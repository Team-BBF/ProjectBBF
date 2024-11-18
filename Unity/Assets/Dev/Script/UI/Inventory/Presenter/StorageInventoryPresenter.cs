using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.EventSystems;

public class StorageInventoryPresenter : MonoBehaviour, IInventoryPresenter<GridInventoryModel>
{
    [SerializeField] private InteractableInventoryView _playerView;
    [SerializeField] private InteractableInventoryView _storageView;
    public GridInventoryModel PlayerModel { get; private set; }
    public GridInventoryModel Model { get; private set; }

    public InteractableInventoryView PlayerView => _playerView;
    public InteractableInventoryView StorageView => _storageView;

    public event Action<StorageInventoryPresenter> OnInit;

    private int _recentSelection;
    
    public void Init(GridInventoryModel model)
    {
        Model = model;
        StartCoroutine(CoInit());
    }

    private IEnumerator CoInit()
    {
        PlayerView.Init();
        StorageView.Init();

        {
            var blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");

            while (blackboard.Inventory?.Model is null)
            {
                yield return null;
            }

            PlayerModel = blackboard.Inventory.Model;
        }
        
        Model.OnChanged += StorageView.Refresh;
        PlayerModel.OnChanged += PlayerView.Refresh;

        PlayerView.OnSlotDown += OnPlayerSlotDown;
        StorageView.OnSlotDown += OnStorageSlotDown;
        
        PlayerView.OnViewClosed += OnClosed;

        PlayerView.Refresh(PlayerModel);
        StorageView.Refresh(Model);
        PlayerView.Visible = false;
        StorageView.Visible = false;

        OnInit?.Invoke(this);
    }

    private void OnClosed()
    {
        var inst = SelectItemPresenter.Instance;
        if (inst)
        {
            if (inst.Model.Selected.Empty is false && _recentSelection != -1)
            {
                Debug.Assert(_recentSelection is not -1);
                var model = _recentSelection is 2 ? Model: PlayerModel;
                int remainCount = model.PushItem(inst.Model.Selected.Data, inst.Model.Selected.Count);

                if (remainCount == inst.Model.Selected.Count)
                {
                    _recentSelection = 0;
                    return;
                }
                
                inst.Model.Selected.Clear();
            }
        }

        _recentSelection = 0;
    }

    private void OnPlayerSlotDown(IInventorySlot obj, PointerEventData eventData)
    {
        if (InputManager.Map.UI.SlotQuickMove.IsPressed() && obj.Data)
        {
            InventoryHelper.QuickMoveWithGridModel(obj, Model);
        }
        else
        {
            _recentSelection = 1;
            InventoryHelper.SwapOrHalfItem(obj, eventData);
        }
    }
    private void OnStorageSlotDown(IInventorySlot obj, PointerEventData eventData)
    {
        if (InputManager.Map.UI.SlotQuickMove.IsPressed() && obj.Data)
        {
            InventoryHelper.QuickMoveWithGridModel(obj, PlayerModel);
        }
        else
        {
            _recentSelection = 2;
            InventoryHelper.SwapOrHalfItem(obj, eventData);
        }
    }

    private void OnDestroy()
    {
        if (Model is not null)
        {
            Model.OnChanged -= StorageView.Refresh;

            if (PlayerModel is not null)
            {
                PlayerModel.OnChanged -= PlayerView.Refresh;
            }
            Model.Release();
        }
    }
}