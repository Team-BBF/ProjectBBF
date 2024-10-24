using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class FishingMinigameController : MinigameBase<FishingMinigameData>
{
    [SerializeField] private GameObject _uiPanel;
    [SerializeField] private GameTimerUI _timerUI;
    [SerializeField] private SpriteRenderer _fishRenderer;

    private float _timer;

    private float Timer
    {
        get => _timer;
        set
        {
            _timer = value;
            _timerUI.Time = new GameTime((int)(Data.GameDuration - _timer));
        }
    }

    protected override void Awake()
    {
        base.Awake();
        OnGameRelease();
    }

    protected override void OnGameInit()
    {
        OnGameRelease();

        Player.Fishing.BindFishingController(this);

        float sum = Data.Rewards.Sum(x => x.Percentage);

        if (Mathf.Approximately(sum, 1f) is false && sum < 1f)
        {
            Debug.LogError($"낚시 보상의 확률 분포가 올바르지 않습니다. FishingMinigameData({Data.name})");
        }

        _fishRenderer.enabled = false;
    }

    protected override async UniTask OnTutorial()
    {
        await RunDialogue(Data.Tutorial);
    }

    protected override void OnGameBegin()
    {
        _timerUI.Visible = true;
        _uiPanel.SetActive(true);
        Player.Blackboard.IsMoveStopped = true;
        Player.HudController.Visible = false;
        Player.VisualStrategy.LookAt(Vector2.right, AnimationActorKey.Action.Idle);
        Player.MoveStrategy.LastMovedDirection = Vector2.right;
        StartCoroutine(CoTimer());
    }

    protected override void OnGameRelease()
    {
        _timerUI.Visible = false;
        _fishRenderer.enabled = false;
        _uiPanel.SetActive(false);
        StopAllCoroutines();
        Timer = 0f;

        if (Player)
        {
            Player.HudController.Visible = true;
            Player.Blackboard.IsMoveStopped = false;
            Player.Fishing.ReleaseFishingController();
        }
    }

    protected override bool IsGameEnd()
    {
        return Player.Fishing.IsFishing is false && Timer >= Data.GameDuration;
    }

    protected override UniTask OnGameEnd(bool isRequestEnd)
    {
        
        if (isRequestEnd) return UniTask.CompletedTask;
        return UniTask.CompletedTask;;
    }

    protected override void OnPreGameEnd(bool isRequestEnd)
    {
        base.OnPreGameEnd(isRequestEnd);
    }

    public FishingContext CreateContext()
    {
        return new FishingContext(_fishRenderer, Data);
    }

    private IEnumerator CoTimer()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            yield return wait;

            if (Timer < Data.GameDuration)
            {
                Timer += 1f;
            }
        }
    }
}