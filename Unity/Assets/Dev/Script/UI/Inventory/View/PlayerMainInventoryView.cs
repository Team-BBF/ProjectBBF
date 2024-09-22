using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerMainInventoryView : MonoBehaviour, IInventoryView
{
    [SerializeField] private ItemToolTipView _toolTipView;
    [SerializeField] private Vector2 _toolTipOffset;
    
    [SerializeField] private Transform _content;
    [SerializeField] private int _colCount = 4;

    private InventorySlotView[,] _slotViews;

    public int Row { get; private set; }
    public int Col { get; private set; }

    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    public void Init()
    {
        _toolTipView.Clear();
        _toolTipView.Visible = false;
        
        int length = _content.childCount;
        Row = length / _colCount + length % _colCount;
        Col = _colCount;
        
        _slotViews = new InventorySlotView
        [
            Row,
            Col
        ];

        int j = 0;
        for (int iter = 0; iter < length; iter++)
        {
            int i = iter / _colCount;

            if (j % _colCount == 0)
            {
                j = 0;
            }

            if (_content.GetChild(iter).TryGetComponent(out InventorySlotView slotView) is false)
            {
                Debug.LogError("컴포넌트가 존재하지 않음");
            }
            else
            {
                _slotViews[i, j] = slotView;
                slotView.OnHoverEnter += OnHoverEnter;
                slotView.OnHoverExit += OnHoverExit;
                slotView.OnMove += OnMove;
                slotView.OnDown += OnDown;
            }

            j++;
        }
    }

    private void OnDown(IInventorySlot obj)
    {
        _toolTipView.Visible = false;
        _toolTipView.Clear();
    }

    private void OnHoverEnter(IInventorySlot slot)
    {
        if (SelectItemPresenter.Instance.Model.IsEmpty is false) return;
        if (slot is null) return;
        if (slot.Data == false) return;
        
        _toolTipView.Visible = true;
        _toolTipView.SetText(slot.Data);
        
    }
    private void OnHoverExit(IInventorySlot slot)
    {
        if (SelectItemPresenter.Instance.Model.IsEmpty is false) return;
        _toolTipView.Visible = false;
        _toolTipView.Clear();
    }

    private void OnMove(IInventorySlot obj, PointerEventData eventData)
    {
        var offset = ItemToolTipView.ToWorldSpaceOffset(_toolTipOffset);

        var pos = ItemToolTipView.ScreenToOrthogonal(eventData.position);
        pos = _toolTipView.ToValidPosition(pos);
        pos = ItemToolTipView.OrthogonalToScreen(pos);

        _toolTipView.transform.position = pos;
    }
    
    public void Refresh(IInventoryModel model)
    {
       using var modelEnumerator = model.GetEnumerator();
        
        for (int i = 0; i < Row; i++)
        {
            for (int j = 0; j < Col; j++)
            {
                if (modelEnumerator.MoveNext() is false) return;
                _slotViews[i, j].SlotController = modelEnumerator.Current;
            }
        }
    }
}
