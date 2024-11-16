


using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashBoxUI : MonoBehaviour
{
    [field: SerializeField] private Button _button;

    [SerializeField] private List<ItemData> _itemBlacklist;
    
    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void OnClick()
    {
        var item = SelectItemPresenter.Instance.Model.Selected;
        if (item is null) return;

        if (item.Data && _itemBlacklist.Contains(item.Data))
        {
            return;
        }
        
        SelectItemPresenter.Instance.Model.Selected.Clear();
    }
}