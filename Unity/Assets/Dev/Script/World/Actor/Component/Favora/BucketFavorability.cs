


using System;
using System.Collections.Generic;
using System.Linq;
using DS.Core;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.Serialization;

public class BucketFavorability : ActorFavorablity
{
    [SerializeField] private DialogueContainer _bucketDialogueContainer;
    [SerializeField] private BucketValidItemTable _bucketValidItemTable;

    [SerializeField] private ItemBucket _bucket;
    public ItemBucket Bucket => _bucket;
    public bool IsEmpty => Bucket.Item == false;

    public const string BUCKET_ITEM_0_NAME_KEY = "bucket_item_0_name";
    public const string BUCKET_ITEM_0_PRICE_KEY = "bucket_item_0_price";
    public bool IsValidBucketItem(ItemData itemData)
    {
        Debug.Assert(_bucketValidItemTable, "Bucket List Item이 없습니다");

        return _bucketValidItemTable.List.Contains(itemData);
    }
    
    public override DialogueEvent DequeueDialogueEvent()
    {
        if (IsEmpty)
        {
            return base.DequeueDialogueEvent();
        }
        
        return new DialogueEvent()
        {
            Container = _bucketDialogueContainer,
            Type = DialogueBranchType.Dialogue,
            ProcessorData = ProcessorData
        };
    }

    public override DialogueEvent PeekDialogueEvent()
    {
        return DequeueDialogueEvent();
    }

    public override void Init(Actor actor)
    {
        base.Init(actor);
        
        _bucket.Item = null;
        _bucket.OnFade(0f);

        SetBucet(null);
    }
    

    public void UpdateBucket(PlayerController playterController)
    {
        IInventorySlot slot = playterController.Inventory.CurrentItemSlot;
        Debug.Assert(slot is not null);
        
        if (InputManager.Map.Player.Interaction.triggered)
        {
            if (_bucket.Item)
            {
                if (slot.Empty)
                {
                    if (SlotChecker.Contains(slot.TrySet(_bucket.Item, 1), SlotStatus.Success) is false)
                    {
                        Debug.LogError("정의되지 않은 동작");
                    }

                    SetBucet(null);
                    return;
                }
            
                if(_bucket.Item == slot.Data)
                {
                    if (SlotChecker.Contains(slot.TryAdd(1), SlotStatus.Success))
                    {
                        SetBucet(null);
                    }

                    return;
                }
            }

            if (IsValidBucketItem(slot.Data))
            {
                SetBucet(slot.Data);
                if (SlotChecker.Contains(slot.TryAdd(-1, true), SlotStatus.Success) is false)
                {
                    SetBucet(null);
                    return;
                }
            }

        }
    }

    private void SetBucet(ItemData itemData)
    {
        // can be null
        _bucket.Item = itemData;
        
        if (IsEmpty)
        {
            ProcessorData.BindingTable[BUCKET_ITEM_0_NAME_KEY] = "None";
            ProcessorData.BindingTable[BUCKET_ITEM_0_PRICE_KEY] = "-1";
        }
        else
        {
            ProcessorData.BindingTable[BUCKET_ITEM_0_NAME_KEY] = itemData.ItemName;
            ProcessorData.BindingTable[BUCKET_ITEM_0_PRICE_KEY] = $"{itemData.Price}";
        }
    }
}