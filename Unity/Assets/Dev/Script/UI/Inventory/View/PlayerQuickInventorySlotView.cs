using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerQuickInventorySlotView : MonoBehaviour
{
    [SerializeField] private Image _slotImage;
    [SerializeField] private TMP_Text _text;
    private IInventorySlot _slotController;

    private void Awake()
    {
        _text.text = "";
        _slotImage.sprite = null;
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
            _slotController.OnChanged += OnChanged;
        }
    }
    public ItemData ItemData => SlotController.Data;

    private void OnChanged(IInventorySlot slot)
    {
        _slotImage.sprite = slot.Data != null ? slot.Data.ItemSprite : null;

        if (slot.Data is not null && slot.Data.ActionCategoryType == ActionCategoryType.Tool)
        {
            _text.text = "";
            return;
        }
        _text.text = slot.Count == 0 ? "" : slot.Count.ToString();
    }
}
