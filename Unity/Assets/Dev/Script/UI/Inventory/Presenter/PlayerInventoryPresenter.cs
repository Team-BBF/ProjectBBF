using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryPresenter : IInventoryPresenter<GridInventoryModel>
{
    public GridInventoryModel Model { get; private set; }
    private PlayerQuickInventoryView _quickView;
    
    private PlayerMainInventoryView _mainView;
    private PlayerPannelView _pannelView;
    
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

    public PlayerInventoryPresenter(GridInventoryModel model, PlayerMainInventoryView mainView, PlayerQuickInventoryView quickView, PlayerPannelView pannelView)
    {
        Model = model;
        
        _mainView = mainView;
        _quickView = quickView;
        
        _pannelView = pannelView;
        
        _mainView.Refresh(Model);
        _quickView.Refresh(Model);

        QuickInvVisible = true;
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
