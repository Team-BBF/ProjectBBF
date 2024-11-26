using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerQuickInventorySlotView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _slotImage;
    [SerializeField] private TMP_Text _text;
    private IInventorySlot _slotController;
    
    public event Action<PlayerQuickInventorySlotView> OnClick;

    private void Awake()
    {
        _text.text = "";
        _slotImage.sprite = null;
    }

    private void OnDestroy()
    {
        SlotController = null;
    }

    public IInventorySlot SlotController
    {
        get => _slotController;
        set
        {
            if (_slotController == value) return;

            if (_slotController is not null)
            {
                _slotController.OnChanged -= OnChanged;
            }
            else
            {
                _text.text = "";
            }

            _slotController = value;

            if (_slotController is not null)
            {
                _slotController.OnChanged += OnChanged;
            }
            OnChanged(_slotController);
        }
    }
    public ItemData ItemData => SlotController.Data;

    private void OnChanged(IInventorySlot slot)
    {
        if (slot is null) return;
        _slotImage.sprite = slot.Data != null ? slot.Data.ItemSprite : null;

        _slotImage.enabled = _slotImage.sprite;

        if (slot.Data is not null && slot.Data.ActionCategoryType == ActionCategoryType.Tool)
        {
            _text.text = "";
            return;
        }
        _text.text = slot.Count == 0 ? "" : slot.Count.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(this);
    }
}
