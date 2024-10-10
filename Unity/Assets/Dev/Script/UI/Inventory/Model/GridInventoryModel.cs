using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridInventorySlot : DefaultInventorySlot
{
    public Vector2Int Position { get; private set; }

    public GridInventorySlot(Vector2Int position)
    {
        this.Position = Position;
    }
}

public class GridInventoryModel : IInventoryModel
{
    public GridInventorySlot[,] Slots { get; private set; }
    public Vector2Int Size { get; private set; }

    public bool IsFull => GetFirstEmptySlotPosition() is null;
    public event Action<ItemData, int, GridInventoryModel> OnPushItem;
    public event Action<ItemData, GridInventoryModel> OnPopItem;

    public event Action<IInventoryModel> OnChanged;
    
    public GridInventoryModel(Vector2Int defaultSize)
    {
        Alloc(defaultSize);
    }

    public void ApplyChanged()
    {
        OnChanged?.Invoke(this);
    }

    private void Alloc(Vector2Int newSize)
    {
        var newSlot = new GridInventorySlot[newSize.y, newSize.x];
        Size = newSize;

        for (int i = 0; i < Size.y; i++)
        {
            for (int j = 0; j < Size.x; j++)
            {
                GridInventorySlot gridSlot = new GridInventorySlot(new Vector2Int(j, i));
                newSlot[i, j] = gridSlot;
                gridSlot.OnChanged += _ => ApplyChanged();
            }
        }

        Slots = newSlot;
    }

    /// <summary>
    /// 인벤토리의 크기를 재할당함.
    /// </summary>
    /// <param name="newSize">재할당할 인벤토리의 크기</param>
    /// <returns>만약, 매개변수의 크기가, 현재 인벤토리 크기보다 작다면 false를 반환함.</returns>
    public bool Realloc(Vector2Int newSize)
    {
        if (newSize.sqrMagnitude < Size.sqrMagnitude)
        {
            return false;
        }

        var originSlot = Slots;
        var newSlot = new GridInventorySlot[newSize.x, newSize.y];

        var eumerator = Slots.GetEnumerator();

        for (int i = 0; i < newSize.y; i++)
        {
            for (int j = 0; j < newSize.x; j++)
            {
                GridInventorySlot gridSlot = new GridInventorySlot(new Vector2Int(j, i));
                newSlot[i, j] = gridSlot;

                if (eumerator.MoveNext() && eumerator.Current is GridInventorySlot originGridSlot)
                {
                    gridSlot.Swap(originGridSlot);
                }
            }
        }

        Slots = newSlot;
        Size = newSize;

        return true;
    }

    /// <summary>
    /// 인벤토리에서 가장 첫번째로 비어있는 슬롯의 포지션을 반환.
    /// </summary>
    /// <returns>인벤토리가 꽉 차있다면 null을 반환.</returns>
    public Vector2Int? GetFirstEmptySlotPosition()
    {
        for (int i = 0; i < Size.y; i++)
        {
            for (int j = 0; j < Size.x; j++)
            {
                if (Slots[i, j].Empty)
                {
                    return new Vector2Int(j, i);
                }
            }
        }

        return null;
    }


    public Vector2Int? GetAdditionalSlotPosition(ItemData itemData)
    {
        for (int i = 0; i < Size.y; i++)
        {
            for (int j = 0; j < Size.x; j++)
            {
                var slot = Slots[i, j];
                if (slot.Data == itemData && slot.Count < slot.Data.MaxStackCount)
                {
                    return new Vector2Int(j, i);
                }
            }
        }

        return null;
    }

    public bool PopItem(ItemData targetItem)
    {
        if (targetItem == false)
        {
            Debug.LogError("itemdata is null");
            return false;
        }
        
        for (int i = MaxSize - 1; i >= 0; i--)
        {
            var slot = GetSlotSequentially(i);

            var result = slot.TryAdd(-1, true);
            if (SlotChecker.Contains(result, SlotStatus.Success))
            {
                OnPopItem?.Invoke(targetItem, this);
                return true;
            }
        }
        return false;
    }
    
    public bool PushItem(ItemData itemData, int count)
    {
        if (itemData == false)
        {
            Debug.LogError("itemdata is null");
            return false;
        }
        
        int remaingCount = count;

        while (remaingCount > 0)
        {
            Vector2Int? slotPos = GetAdditionalSlotPosition(itemData);
            InventorySlotSetMethod method;

            if (slotPos is not null)
            {
                method = InventorySlotSetMethod.Add;
            }
            else
            {
                slotPos = GetFirstEmptySlotPosition();

                if (slotPos is null) return false;
                method = InventorySlotSetMethod.Set;
            }


            var slot = Slots[slotPos!.Value.y, slotPos!.Value.x];

            SlotStatus status;

            if (method == InventorySlotSetMethod.Add)
            {
                status = slot.TryAdd(remaingCount);
            }
            else
            {
                status = slot.TrySet(itemData, remaingCount);
            }

            if (SlotChecker.Contains(status, SlotStatus.OverMaxStack))
            {
                remaingCount -= (itemData.MaxStackCount - slot.Count);
                slot.ForceSet(itemData, itemData.MaxStackCount);
                OnPushItem?.Invoke(itemData, itemData.MaxStackCount, this);
                continue;
            }

            if (SlotChecker.Contains(status, SlotStatus.Success))
            {
                OnPushItem?.Invoke(itemData, remaingCount, this);
                return true;
            }

        }
        
        return false;
    }

    public int MaxSize => Size.x * Size.y;

    public IInventorySlot GetSlotSequentially(int index)
    {
        int y = index / Size.x;
        int x = index % Size.x;

        if (index < 0 || index >= MaxSize) return null;

        return Slots[y, x];
    }

    public IEnumerator<IInventorySlot> GetEnumerator()
    {
        for (int i = 0; i < MaxSize; i++)
        {
            var slot = GetSlotSequentially(i);

            yield return slot;
        }
    }

    public bool Contains(ItemData itemData)
    {
        var eumerator = Slots.GetEnumerator();
        while (eumerator.MoveNext())
        {
            if (eumerator.Current is IInventorySlot slot && itemData == slot.Data)
            {
                return true;
            }
        }

        return false;
    }

    public bool ContainsGreaterEqual(ItemData itemData, int count)
    {
        var eumerator = Slots.GetEnumerator();
        int curCount = 0;
        
        while (eumerator.MoveNext())
        {
            if (eumerator.Current is IInventorySlot slot && itemData == slot.Data)
            {
                curCount += slot.Count;

                if (curCount >= count)
                {
                    return true;
                }
            }
            
        }

        return false;
    } 
}