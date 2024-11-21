using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerQuickInventoryView : MonoBehaviour, IInventoryView
{
    [SerializeField] private PlayerQuickInventorySlotView[] _slots;
    [SerializeField] private RectTransform _cursor;
    private int _currentCursor;

    public int CurrentItemIndex
    {
        get => _currentCursor;
        set
        {
            _cursor.gameObject.SetActive(true);
            _currentCursor = Mathf.Clamp(value, 0, _slots.Length - 1);
            _cursor.position = _slots[_currentCursor].transform.As<RectTransform>().position;
        }

    }
    public bool CursorMoveLock { get; set; }
    public int MaxSlotCount => _slots.Length;

    private PlayerBlackboard _blackboard;

    public IInventorySlot GetSlotAt(int cursorIndex)
    {
        return _slots[cursorIndex].SlotController;
    }

    private void Awake()
    {
        _cursor.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        OnLoadedData(PersistenceManager.Instance);
        PersistenceManager.Instance.OnGameDataLoaded += OnLoadedData;
        InputManager.Map.UI.QuickSlotScroll.performed += MoveCursorScroll;
        InputManager.Map.UI.QuickSlotScrollButton.performed += MoveCursorButton;
    }

    private void OnLoadedData(PersistenceManager inst)
    {
        _blackboard = inst.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
    }

    private void OnDisable()
    {
        if (PersistenceManager.Instance)
        {
            PersistenceManager.Instance.OnGameDataLoaded -= OnLoadedData;
        }
        
        InputManager.Map.UI.QuickSlotScroll.performed -= MoveCursorScroll;
        InputManager.Map.UI.QuickSlotScrollButton.performed -= MoveCursorButton;
    }
    
    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    private void MoveCursorScroll(InputAction.CallbackContext ctx)
    {
        if (CursorMoveLock) return;
        
        float fscrollValue = ctx.ReadValue<float>();

        int value = 0;

        if (fscrollValue > 0f) value = 1;
        else if (fscrollValue < 0f) value = -1;
        else return;
        
        
        _cursor.gameObject.SetActive(true);

        if (value + CurrentItemIndex >= _slots.Length)
        {
            CurrentItemIndex = 0;
        }
        else if (value + CurrentItemIndex < 0)
        {
            CurrentItemIndex = _slots.Length - 1;
        }
        else
        {
            CurrentItemIndex += value;
        }
        

        AudioManager.Instance.PlayOneShot("UI", "UI_Tool_Swap");
    }

    private void MoveCursorButton(InputAction.CallbackContext ctx)
    {
        if (CursorMoveLock) return;
        
        float fscrollValue = ctx.ReadValue<float>();

        int value = Mathf.RoundToInt(fscrollValue);

        _cursor.gameObject.SetActive(true);
        CurrentItemIndex = value -1 ;

        AudioManager.Instance.PlayOneShot("UI", "UI_Tool_Swap");
        
        _cursor.position = (_slots[_currentCursor].transform as RectTransform)!.position;
    }

    public void Refresh(IInventoryModel model)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            var slot = model.GetSlotSequentially(i);
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
