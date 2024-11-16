using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInventoryPresenter : IInventoryPresenter<GridInventoryModel>
{
    public GridInventoryModel Model { get; private set; }
    private PlayerQuickInventoryView _quickView;
    
    private InteractableInventoryView _mainView;
    private PlayerPannelView _pannelView;

    public PlayerQuickInventoryView QuickView => _quickView;

    public bool QuickInvVisible
    {
        get => _quickView.Visible;
        set => _quickView.Visible = value;
    }
    
    public bool MainInvVisible
    {
        get => _pannelView.ViewState == PlayerPannelView.ViewType.Inv;
        set => _pannelView.ViewState = value ? PlayerPannelView.ViewType.Inv : PlayerPannelView.ViewType.Close;
    }

    public PlayerInventoryPresenter(GridInventoryModel model, InteractableInventoryView mainView, PlayerQuickInventoryView quickView, PlayerPannelView pannelView)
    {
        Model = model;
        
        _mainView = mainView;
        _quickView = quickView;
        
        _pannelView = pannelView;

        _mainView.OnSlotDown += OnSlotDown;
        _mainView.OnViewClosed += OnClosed;
        
        _mainView.Refresh(Model);
        _quickView.Refresh(Model);

        QuickInvVisible = true;
    }

    private void OnClosed()
    {
        var inst = SelectItemPresenter.Instance;
        if (inst)
        {
            if (inst.Model.Selected.Empty is false)
            {
                int remainCount = Model.PushItem(inst.Model.Selected.Data, inst.Model.Selected.Count);

                if (remainCount == inst.Model.Selected.Count)
                {
                    return;
                }
                
                inst.Model.Selected.Clear();
            }
        }
    }

    private void OnSlotDown(IInventorySlot obj, PointerEventData eventData)
    {
        InventoryHelper.SwapOrHalfItem(obj, eventData);
    }

    public ItemData CurrentItemData => CurrentItemSlot?.Data;
    public IInventorySlot CurrentItemSlot =>  Model.GetSlotSequentially(_quickView.CurrentItemIndex);

    public void Refresh()
    {
        _mainView.Refresh(Model);
        _quickView.Refresh(Model);
    }

    public void HideAll()
    {
        MainInvVisible = false;
        QuickInvVisible = false;
    }
}
