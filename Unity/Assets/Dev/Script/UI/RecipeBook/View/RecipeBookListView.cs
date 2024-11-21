using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.UI;

public class RecipeBookListView : MonoBehaviour
{
    [SerializeField] private Image _selectRect;
    [SerializeField] private Image _hoverRect;
    [SerializeField] private bool _awakeAndDisable = true;
    
    [SerializeField] private GameObject _slotViewPrefab;
    [SerializeField] private Transform _content;

    [SerializeField] private ESOVoid _closeEvent;
    [SerializeField] private ESOVoid _closeReadyEvent;
    
    public event Action<object> OnSlotClick;
    //public event Action OnExit;

    private List<RecipeBookSlotView> _slots = new();

    public List<RecipeBookSlotView> Slots => _slots;

    private bool _canCloseEventRaise;

    public bool Visible
    {
        get => gameObject.activeSelf;
        set
        {
            gameObject.SetActive(value);

            if (_canCloseEventRaise && value is false && _closeEvent)
            {
                _canCloseEventRaise = false;
                _closeEvent.Raise();
            }
        }
    }

    private void OnEventReady()
    {
        _canCloseEventRaise = true;
    }
    
    private void Awake()
    {
        Visible = !_awakeAndDisable;

        if(_closeReadyEvent)
            _closeReadyEvent.OnEventRaised += OnEventReady;

        for (int i = 0; i < _content.childCount; i++)
        {
            Destroy(_content.GetChild(i).gameObject);
        }
        
        _selectRect.gameObject.SetActive(false);
        _hoverRect.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (var slot in _slots)
        {
            slot.OnClick -= OnSlotClick;
            slot.OnHoverEnter += OnSlotEnter;
            slot.OnHoverExit+= OnSlotExit;
        }
        _slots.Clear();
        

        if(_closeReadyEvent)
            _closeReadyEvent.OnEventRaised -= OnEventReady;
    }

    private RecipeBookSlotView CreateSlotView()
    {
        var obj = Instantiate(_slotViewPrefab, _content);
        var slotView = obj.GetComponent<RecipeBookSlotView>();
        
        Debug.Assert(slotView);

        return slotView;
    }

    public void AddItem(Sprite itemSprite, object data, bool isUnlocked)
    {
        var slot = CreateSlotView();
        slot.SetData(itemSprite, data, isUnlocked);
        slot.OnClick += OnSlotClicked;
        slot.OnHoverEnter += OnSlotEnter;
        slot.OnHoverExit += OnSlotExit;
        _slots.Add(slot);
    }

    public void UpdateBookmark(object recipeData)
    {
        _slots.ForEach(x=>x.UpdateBookmark(recipeData));
    }

    private void OnSlotExit(RecipeBookSlotView obj)
    {
        _hoverRect.gameObject.SetActive(false);   
    }

    private void OnSlotEnter(RecipeBookSlotView slotView)
    {
        _hoverRect.gameObject.SetActive(true);
        _hoverRect.transform.position = slotView.transform.position;
    }

    private void OnSlotClicked(RecipeBookSlotView slotView)
    {
        _selectRect.gameObject.SetActive(true);
        _selectRect.transform.position = slotView.transform.position;
        OnSlotClick?.Invoke(slotView.Data);
    }
}
