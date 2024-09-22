using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectItemView : MonoBehaviour, IInventoryView
{
    [SerializeField] private Image _selectedImage;
    [SerializeField] private TMP_Text _countText;

    public Sprite Sprite
    {
        get => _selectedImage.sprite;
        set
        {
            _selectedImage.sprite = value;
            gameObject.SetActive(value);

            UpdatePosition();
        }
    }

    public int Count
    {
        set =>_countText.text = value.ToString();
    }
    
    private void UpdatePosition()
    {
        var rectTransform = _selectedImage.transform as RectTransform;
        if (rectTransform is null) return;
        
        RectTransformUtility
            .ScreenPointToWorldPointInRectangle(
                rectTransform,
                Input.mousePosition, 
                null, 
                out Vector3 worldPoint);
        
        rectTransform.position = worldPoint;
    }

    private void Update()
    {
        UpdatePosition();
    }

    public void Refresh(IInventoryModel model)
    {
        OnChanged(model.GetSlotSequentially(0));
    }

    public void OnChanged(IInventorySlot slot)
    {
        Sprite = slot.Data?.ItemSprite;
        Count = slot.Count;
        UpdatePosition();
    }
}