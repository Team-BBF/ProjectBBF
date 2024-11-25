using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DS.Runtime;
using ProjectBBF.Input;
using ProjectBBF.Persistence;
using UnityEngine;
using DialogueBranchField = DS.Runtime.DialogueBranchField;

public class FrogRaceMinigameController : MinigameBase<FrogRaceMinigameData>
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private List<Transform> _tracks;

    private List<FrogRaceFrogObject> _frogs;

    private int _goalIndex;
    private int _targetIndex;
    private int _money;

    private CinemachineBrain _brain;

    protected override void Awake()
    {
        base.Awake();
        
        _camera.gameObject.SetActive(false);
    }

    protected override void OnGameInit()
    {
        _frogs = new List<FrogRaceFrogObject>(Data.Frogs.Length);
        _goalIndex = 0;
        _targetIndex = 0;
        _money = 0;

        using var enumerator = _tracks.GetEnumerator();
        foreach (var data in Data.Frogs)
        {
            if (enumerator.MoveNext() is false)
            {
                Debug.LogError("트랙이 모자람");
                return;
            }

            var obj = Instantiate(data.FrogObject, transform);
            obj.FrogData = data;
            obj.gameObject.SetActive(true);
            obj.transform.position = enumerator.Current!.position;
            _frogs.Add(obj);
        }

        _camera.gameObject.SetActive(true);
        _camera.MoveToTopOfPrioritySubqueue();
        _brain = Camera.main.GetComponent<CinemachineBrain>();
        _brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        
        Player.MoveStrategy.ResetVelocity();
        Player.Blackboard.IsMoveStopped = true;
        Player.Blackboard.IsInteractionStopped = true;
        Player.Inventory.QuickInvVisible = false;
        Player.QuestPresenter.Visible = false;
        Player.RecipeSummaryView.Visible = false;

        Player.InputController.BindInput(InputAbstractFactory.CreateFactory<PlayerController, DefaultPlayerInputFactory>(Player));
        Player.InputController.Move.Value = null;
        Player.InputController.Interact.Value = null;
        Player.InputController.Tool.Value = null;
    }

    protected override async UniTask OnTutorial()
    {
        DialogueController.Instance.ResetDialogue();
        
#if !STOVE_BUILD
        var inst = DialogueController.Instance;

        var fields = _frogs.Select(x => inst.GetField<BranchFieldButton>().Init(x.FrogData.DisplayName)).ToArray();
        var result = await inst.GetBranchResultAsync(
            fields, this.GetCancellationTokenOnDestroy()
        );

        _targetIndex = result.index;
        
        Debug.Assert(_targetIndex is not -1);

        while (true)
        {
            Player.HudController.Visible = true;
            
            
            BranchFieldIntInput moneyInput;
            result = await inst.GetBranchResultAsync(
                new DialogueBranchField[]
                {
                    moneyInput = inst.GetField<BranchFieldIntInput>().Init(100, 100, 100, 1000),
                    inst.GetField<BranchFieldButton>().Init("경기 시작하기"),
                    inst.GetField<BranchFieldButton>().Init("그만두기")
                }, this.GetCancellationTokenOnDestroy()
            );

            _money = moneyInput.GetResult().Value;

            if (result.index == 2)
            {
                _money = 0;
                RequestEndGame();
                break;
            }

            if (Player.Blackboard.Money < _money)
            {
                DialogueController.Instance.DialogueText = "돈이 부족하군.";
                await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update);
                DialogueController.Instance.ResetDialogue();
                continue;
            }

            break;
        }

        DialogueController.Instance.ResetDialogue();
#endif
    }

    protected override void OnGameBegin()
    {
        Player.HudController.Visible = false;
        Player.Inventory.QuickInvVisible = false;
        Player.QuestPresenter.Visible = false;
        Player.RecipeSummaryView.Visible = false;
        StartCoroutine(CoUpdate());

        for (int i = 0; i < _frogs.Count; i++)
        {
            var frog = _frogs[i];
            frog.Begin(Data, Data.Frogs[i]);
        }
    }

    private IEnumerator CoUpdate()
    {
        while (true)
        {
            yield return null;
        }
    }

    protected override void OnGameRelease()
    {
        _camera.gameObject.SetActive(false);
        foreach (var frog in _frogs)
        {
            frog.End();
            frog.gameObject.SetActive(false);
            Destroy(frog.gameObject);
        }

        _frogs.Clear();
    }

    protected override void OnPreGameEnd(bool isRequestEnd)
    {
        _camera.gameObject.SetActive(false);
    }

    protected override bool IsGameEnd()
    {
        for (int i = 0; i < _frogs.Count; i++)
        {
            var frog = _frogs[i];
            if (frog.IsGoal)
            {
                _goalIndex = i;

                for (int j = 0; j < _frogs.Count; j++)
                {
                    _frogs[j].IsStop = true;
                }
                
                return true;
            }
        }

        return false;
    }

    protected override async UniTask OnGameEnd(bool isRequestEnd)
    {

        Player.MoveStrategy.ResetVelocity();
        
        
#if !STOVE_BUILD
        var blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
        var inst = DialogueController.Instance;
        inst.ResetDialogue();
        inst.Visible = true;

        if (_goalIndex == _targetIndex)
        {
            AudioManager.Instance.PlayOneShot("SFX", "SFX_Frog_Win");
            AudioManager.Instance.PlayOneShot("Player", "Player_Getting_Coin");
            
            Debug.Assert(_targetIndex is not -1 && _frogs.Count > 0, $"{_frogs.Count}, {_targetIndex}");
            var frogData = _frogs[_targetIndex];
            int money = (int)(frogData.FrogData.DividendRate * _money);
            blackboard.Money += money;
            inst.DialogueText = $"<color=#826209>{money}</color> 원 벌었습니다.";
        }
        else
        {
            AudioManager.Instance.PlayOneShot("SFX", "SFX_Frog_Lose");
            inst.DialogueText = $"돈을 잃었습니다.";
            blackboard.Money = Mathf.Max(0, blackboard.Money - _money);
        }

        OnGameRelease();
        await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update);
        inst.ResetDialogue();
#else
        OnGameRelease();
#endif

        Player.Blackboard.IsMoveStopped = false;
        Player.Blackboard.IsInteractionStopped = false;
        Player.Inventory.QuickInvVisible = true;
        Player.HudController.Visible = true;
        Player.HudController.SetAllHudVisible(true);
        Player.QuestPresenter.Visible = true;
        Player.RecipeSummaryView.Visible = true;
        Player.InputController.BindInput(InputAbstractFactory.CreateFactory<PlayerController, DefaultPlayerInputFactory>(Player));
        
    }
}