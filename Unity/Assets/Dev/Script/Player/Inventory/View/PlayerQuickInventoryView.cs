using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerQuickInventoryView : MonoBehaviour, IInventoryView
{
    [SerializeField] private PlayerQuickInventorySlotView[] _slots;
    [SerializeField] private RectTransform _cursor;
    private int _currentCursor;

    public int CurrentItemIndex => _currentCursor;
    public int MaxSlotCount => _slots.Length;
    private IInventory _targetInventory;

    public void Init(IInventory targetInventory)
    {
        _targetInventory = targetInventory;
        Refresh();
    }

    private void OnEnable()
    {
        InputManager.Actions.QuickSlotScroll.performed += MoveCursorScroll;
        InputManager.Actions.QuickSlotScrollButton.performed += MoveCursorButton;
    }

    private void OnDisable()
    {
        InputManager.Actions.QuickSlotScroll.performed -= MoveCursorScroll;
        InputManager.Actions.QuickSlotScrollButton.performed -= MoveCursorButton;
    }
    
    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    private void MoveCursorScroll(InputAction.CallbackContext ctx)
    {
        float fscrollValue = ctx.ReadValue<float>();

        int value = 0;

        if (fscrollValue > 0f) value = 1;
        else if (fscrollValue < 0f) value = -1;
        
        _currentCursor = Mathf.Clamp(_currentCursor + value, 0, _slots.Length - 1);
        
        _cursor.position = (_slots[_currentCursor].transform as RectTransform)!.position;
    }

    private void MoveCursorButton(InputAction.CallbackContext ctx)
    {
        float fscrollValue = ctx.ReadValue<float>();

        int value = Mathf.RoundToInt(fscrollValue);

        _currentCursor = Mathf.Clamp(value - 1, 0, _slots.Length - 1);
        
        _cursor.position = (_slots[_currentCursor].transform as RectTransform)!.position;
    }

    public void Refresh()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            var slot = _targetInventory.GetSlotSequentially(i);
            if (slot is null)
            {
                _slots[i].SlotController = null;
                return;
            }

            _slots[i].SlotController = slot;
        }
        
        _cursor.position = (_slots[_currentCursor].transform as RectTransform)!.position;
    }
}
