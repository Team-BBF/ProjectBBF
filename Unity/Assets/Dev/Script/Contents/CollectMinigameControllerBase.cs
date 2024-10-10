using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class CollectMinigameControllerBase<T> : MinigameBase<T> where T : CollectMinigameDataBase
{
    protected int CurrentItemCount { get; private set; }
    
    protected override void OnGameInit()
    {
        Player.Inventory.Model.OnPushItem += OnItemCount;
        CurrentItemCount = 0;
    }
    protected override void OnGameRelease()
    {
        Player.Inventory.Model.OnPushItem -= OnItemCount;
        CurrentItemCount = 0;
        
    }
    protected override bool IsGameEnd()
    {
        return CurrentItemCount >= Data.GoalItemCount;
    }

    private void OnItemCount(ItemData itemData, int count, GridInventoryModel model)
    {
        if (itemData == false) return;
        if (itemData == Data.GoalItem)
        {
            CurrentItemCount += count;
        }
    }
}


public abstract class CollectMinigameDataBase : MinigameData
{
    [SerializeField] private int _goalItemCount;
    [SerializeField] private ItemData _gloalItem;

    public int GoalItemCount => _goalItemCount;
    public ItemData GoalItem => _gloalItem;
}

