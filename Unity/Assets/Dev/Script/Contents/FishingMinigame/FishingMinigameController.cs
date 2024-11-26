using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using ProjectBBF.Input;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class FishingMinigameController : MinigameBase<FishingMinigameData>
{
    [SerializeField] private ItemData _fishingRodItem;
    [SerializeField] private GameObject _uiPanel;
    [SerializeField] private GameTimerUI _timerUI;
    [SerializeField] private SpriteRenderer _fishRenderer;

    private float _timer;

    private IInventorySlot _fishingSlot = null;
    private IInventorySlot _targetSwapSlot = null;
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
        Player.Blackboard.IsMoveStopped = true;
        Player.Blackboard.IsFishingStopped = false;
        Player.HudController.Visible = false;
        Player.Inventory.QuickInvVisible = false;
        Player.VisualStrategy.SetIdle(Vector2.right);
        Player.MoveStrategy.LastMovedDirection = Vector2.right;

        if (_fishingRodItem)
        {
            Player.Inventory.Model.PushItem(_fishingRodItem, 1);
        }
        
        using IEnumerator<IInventorySlot> slotEnumerator = Player.Inventory.Model.GetEnumerator();

        
        
        while (slotEnumerator.MoveNext())
        {
            if (slotEnumerator.Current is not null && slotEnumerator.Current.Data == _fishingRodItem)
            {
                _fishingSlot = slotEnumerator.Current;
                break;
            }
        }
        
        Debug.Assert(_fishingSlot is not null, "플레이어에게서 낚시대를 찾을 수 없습니다.");
        
        _targetSwapSlot = Player.Inventory.Model.GetSlotSequentially(0);
        Debug.Assert(_targetSwapSlot is not null);
        
        
        if (_fishingSlot is not null && _targetSwapSlot is not null)
        {
            _fishingSlot.Swap(_targetSwapSlot);
        }
        
        Player.Inventory.QuickView.CurrentItemIndex = 0;
        Player.Inventory.QuickView.CursorMoveLock = true;

        Player.InputController.BindInput(InputAbstractFactory.CreateFactory<PlayerController, DefaultPlayerInputFactory>(Player));
        Player.InputController.Move.Value = null;
        Player.InputController.Interact.Value = null;
        Player.InputController.UI.Value = null;
        
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
        StartCoroutine(CoTimer());
        StartCoroutine(CoFirstClickCursor());
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
            Player.Inventory.QuickInvVisible = true;
            Player.Inventory.QuickView.CursorMoveLock = false;
            Player.Fishing.ReleaseFishingController();
            Player.InputController.BindInput(InputAbstractFactory.CreateFactory<PlayerController, DefaultPlayerInputFactory>(Player));
            
            if (_fishingSlot is not null && _targetSwapSlot is not null)
            {
                _fishingSlot.Swap(_targetSwapSlot);
                _fishingSlot.Clear();
            }
        }
        
        
        _fishingSlot = null;
        _targetSwapSlot = null;
    }

    protected override bool IsGameEnd()
    {
        bool isEnd = Player.Fishing.IsFishing is false && Timer >= Data.GameDuration;
        if (isEnd)
        {
            Player.InputController.Tool.Value = null;
        }
        
        return isEnd;
    }

    protected override UniTask OnGameEnd(bool isRequestEnd)
    {
        Player.Interactor.CHRO_FISHING = false;
        Player.Blackboard.IsFishingStopped = true;
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

    private IEnumerator CoFirstClickCursor()
    {
        bool flip = false;
        Player.Interactor.CHRO_FISHING = true;
        while (ScreenManager.Instance)
        {
            if(Player == false)break;
            if (Player.Fishing.IsFishing) break;
            
            if (flip)
            {
                ScreenManager.Instance.SetCursorForce(CursorType.CanClick);
            }
            else
            {
                ScreenManager.Instance.SetCursorForce(CursorType.ClickEmpty);  
            }

            flip = !flip;
            
            yield return new WaitForSeconds(0.25f);
        }

        Player.Interactor.CHRO_FISHING = false;
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