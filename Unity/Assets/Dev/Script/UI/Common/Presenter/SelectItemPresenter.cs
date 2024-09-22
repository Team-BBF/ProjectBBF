using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;

[Singleton(ESingletonType.Global)]
public class SelectItemPresenter : MonoBehaviourSingleton<SelectItemPresenter>, IInventoryPresenter<SelectItemModel>
{
    private SelectItemView _view;
    public SelectItemModel Model { get; private set; }

    public override void PostInitialize()
    {
        var temp = Resources.Load<SelectItemView>("Feature/SelectItemView");
        
        _view = Instantiate(temp, transform, true);
        Model = new SelectItemModel();

        _view.Sprite = null;
        _view.Count = 0;
        Model.Selected.OnChanged += _view.OnChanged;
    }

    public override void PostRelease()
    {
        Model.Selected.OnChanged -= _view.OnChanged;
        _view = null;
        Model = null;
    }
    
}