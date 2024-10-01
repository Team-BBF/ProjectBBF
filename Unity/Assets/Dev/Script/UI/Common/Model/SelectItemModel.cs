﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectItemModel : IInventoryModel
{
    public int MaxSize => 1;
    
    public readonly DefaultInventorySlot Selected = new();
    public bool IsEmpty => Selected.Empty;

    public SelectItemModel()
    {
        Selected.OnChanged += _ => ApplyChanged();
    }

    public IInventorySlot GetSlotSequentially(int index)
    {
        if (index > 0 || index < 0) return null;

        return Selected;
    }

    public IEnumerator<IInventorySlot> GetEnumerator()
    {
        yield return Selected;
    }

    public bool Contains(ItemData itemData)
    {
        if (Selected is null || itemData is null) return false;
        
        return Selected.Data == itemData;
    }

    public event Action<IInventoryModel> OnChanged;
    public void ApplyChanged()
    {
        OnChanged?.Invoke(this);
    }
}