using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private Image _slotImage;
    [SerializeField] private TMP_Text _text;
    private IInventorySlot _slotController;

    public event Action<IInventorySlot, PointerEventData> OnDown;
    public event Action<IInventorySlot, PointerEventData> OnHoverEnter;
    public event Action<IInventorySlot, PointerEventData> OnHoverExit;
    public event Action<IInventorySlot, PointerEventData> OnMove;

    private void Awake()
    {
        if (_slotController is not null) return;
        
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

        _slotImage.SetAlpha(slot.Data ? 1f : 0f);

        if (slot.Data is not null && slot.Data.ActionCategoryType == ActionCategoryType.Tool)
        {
            _text.text = "";
            return;
        }

        _text.text = slot.Count == 0 ? "" : slot.Count.ToString();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.PlayOneShot("UI", "UI_MouseOver");
        OnDown?.Invoke(SlotController, eventData);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke(SlotController, eventData);

        if (SlotController?.Data is not null)
        {
            AudioManager.Instance.PlayOneShot("UI", "UI_MouseOver");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke(SlotController, eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        OnMove?.Invoke(SlotController, eventData);
    }
}