using System;
using System.Collections;
using System.Linq;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BakeryPressed: BakeryFlowBehaviourBucket
{
    [SerializeField] private float _endWait;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Transform _playPoint;
    [SerializeField] private Transform _revertPoint;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ESOQuest _esoQuest;
    [SerializeField] private QuestData _questAdditiveData;
    [SerializeField] private QuestData _questOvenData;
    [SerializeField] private QuestData _questDoughData;
    [SerializeField] private GameObject _activationUI;
    [SerializeField] private GameObject _particleSystem;

    private void Start()
    {
        SetParticleVisible(false);
        _panel.SetActive(false);
        _activationUI.SetActive(false);
    }

    private void SetParticleVisible(bool value)
    {
        if (_particleSystem == false) return;
        
        _particleSystem.SetActive(value);
        if (value is false) return;
        
        foreach (var particle in _particleSystem.GetComponentsInChildren<ParticleSystem>())
        {
            particle.Play();
        }
    }

    protected override void OnActivate(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) return;
        if (IsFullBucket is false) return;

        StartCoroutine(CoUpdate(pc));
    }

    protected override void OnChangedBuket(int index, ItemData itemData)
    {
        base.OnChangedBuket(index, itemData);

        for (int i = 0; i < BucketLength; i++)
        {
            if (GetBucket(i) == false)
            {
                _activationUI.SetActive(false);
                return;
            }
        }
        
        _activationUI.SetActive(true);
    }

    protected override void OnEnter(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (IsFullBucket)
        {
            _activationUI.SetActive(true);
        }
    }

    protected override void OnExit(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        _activationUI.SetActive(false);
        GameReset();
        StopAllCoroutines();
    }

    private IEnumerator CoUpdate(PlayerController pc)
    {
        float t = 0f;
        InputAction keyAction = InputManager.Map.Minigame.BakeryKeyPressed;
        
        _panel.SetActive(true);
        _fillImage.fillAmount = 0f;
        AnimationActorKey.Action aniAction;
        
        
        _activationUI.SetActive(false);

        
        SetParticleVisible(true);

        switch (ResolvorType)
        {
            case Resolvor.Dough:
                aniAction = AnimationActorKey.Action.Bakery_Knead;
                break;
            case Resolvor.Additive:
                aniAction = AnimationActorKey.Action.Bakery_Additive;
                break;
            case Resolvor.Baking:
                aniAction = AnimationActorKey.Action.Bakery_Oven;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        (ItemData failItem, ItemData resultItem, float duration, BakeryRecipeData recipeData) tuple = GetResolvedItem();

        pc.Blackboard.IsMoveStopped = true;
        pc.Blackboard.IsInteractionStopped = true;
        QuestIndicator.Visible = false;
        float backupPcZ = pc.transform.position.z;
        pc.transform.position = _playPoint.position;
        pc.MoveStrategy.ResetVelocity();
        pc.MoveStrategy.IsGhost = true;

        yield return null;
        pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(aniAction, AnimationActorKey.Direction.Down), true);
        pc.MoveStrategy.LastMovedDirection = Vector2.down;


        if (ResolvorType == Resolvor.Dough)
        {
            var clip = AudioManager.Instance.GetAudio("SFX", "SFX_Bakery_Kneading").clip;
            _audioSource.clip = clip;
            _audioSource.Play();
        }
        else if (ResolvorType == Resolvor.Additive)
        {
            var clip = AudioManager.Instance.GetAudio("SFX", "SFX_Bakery_Baking").clip;
            _audioSource.clip = clip;
            _audioSource.Play();
            _audioSource.loop = true;
        }

        while (true)
        {
            if (keyAction.IsPressed() is false)
            {
                GameReset();
                
                pc.MoveStrategy.IsStopped = false;
                pc.Blackboard.IsInteractionStopped = false;
                QuestIndicator.Visible = true;
                pc.MoveStrategy.ResetVelocity();
                pc.MoveStrategy.IsGhost = false;
                pc.transform.position = _revertPoint.position;
                pc.transform.SetZ(backupPcZ);
                pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Down), true);

                if (_activationUI.activeSelf == false)
                {
                    _activationUI.SetActive(true);
                }

                _audioSource.loop = false;
                yield break;
            }

            pc.transform.position = _playPoint.position;
            _fillImage.fillAmount = t / tuple.duration;
            t += Time.deltaTime;

            if (t > tuple.duration)
            {
                break;
            }

            yield return null;
        }
        
        yield return null;
        
        _audioSource.loop = false;

        Sprite privewItem;
        
        if (tuple.resultItem is not null)
        {
            AudioManager.Instance.PlayOneShot("SFX", "SFX_Bakery_BakingComplete");
            privewItem = tuple.resultItem.ItemSprite;
            GameSuccess(tuple, pc);
        }
        else
        {
            privewItem = tuple.failItem.ItemSprite;
            GameFail(tuple, pc);
        }
        
        pc.MoveStrategy.IsGhost = false;
        pc.transform.position = _revertPoint.position;

        _panel.SetActive(false);
        pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Bakery_Additive_Complete, AnimationActorKey.Direction.Down), true);
        pc.Interactor.ItemPreviewSprite = privewItem;
        yield return new WaitForSeconds(_endWait);
        pc.Interactor.ItemPreviewSprite = null;

        pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Down), true);
        
        
        QuestIndicator.Visible = true;
        pc.Blackboard.IsMoveStopped = false;
        pc.Blackboard.IsInteractionStopped = false;
        pc.MoveStrategy.ResetVelocity();
        pc.transform.SetZ(backupPcZ);
        _audioSource.Stop();

        _activationUI.SetActive(false);
        GameReset();
    }

    private void GameReset()
    {
        SetParticleVisible(false);
        StopAllCoroutines();
        _panel.SetActive(false);
        _audioSource.Stop();
    }

    private void GameSuccess((ItemData failItem, ItemData resultItem, float duration, BakeryRecipeData recipeData) tuple, PlayerController pc)
    {
        if (tuple.resultItem == false) return;
        
        bool success = pc.Inventory.Model.PushItem(tuple.resultItem, 1) is 0;
        if (success is false) return;
        

        if (tuple.recipeData)
        {
            pc.RecipeBookPresenter.Model.Add(tuple.recipeData.Key);
        }

        if (_esoQuest)
        {
            if (ResolvorType == Resolvor.Additive && _questAdditiveData)
            {
                _esoQuest.Raise(new QuestEvent
                {
                    Type = QuestType.Complete,
                    QuestKey = _questAdditiveData.QuestKey
                });
            }
            if (ResolvorType == Resolvor.Dough && _questOvenData && _questDoughData)
            {
                _esoQuest.Raise(new QuestEvent
                {
                    Type = QuestType.Create,
                    QuestKey = _questOvenData.QuestKey
                });
                _esoQuest.Raise(new QuestEvent
                {
                    Type = QuestType.Complete,
                    QuestKey = _questDoughData.QuestKey
                });
            }
        }
        ClearBucket();
    }
    private void GameFail((ItemData failItem, ItemData resultItem, float duration, BakeryRecipeData recipeData) tuple, PlayerController pc)
    {
        if (_esoQuest)
        {
            // 연성 단계고, 연성과 오븐 퀘스트 데이터를 찾을 수 있다면 퀘스트 발행/실패 처리
            if (ResolvorType == Resolvor.Additive && _questAdditiveData && _questOvenData)
            {
                _esoQuest.Raise(new QuestEvent
                {
                    Type = QuestType.Create,
                    QuestKey = _questOvenData.QuestKey
                });
                _esoQuest.Raise(new QuestEvent
                {
                    Type = QuestType.Cancele,
                    QuestKey = _questAdditiveData.QuestKey
                });
            }
        }
        
        if (tuple.failItem == false) return;
        
        bool success = pc.Inventory.Model.PushItem(tuple.failItem, 1) is 0;
        if (success is false) return;
        
        ClearBucket();
    }
}