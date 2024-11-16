


using System.Collections.Generic;
using ProjectBBF.Event;
using UnityEngine;

public class MapItemGiverReceiver : MonoBehaviour
{
    [SerializeField] private List<ItemDataSerializedSet> _itemList;
    
    public void GiveItemToPlayer(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        
        foreach (ItemDataSerializedSet item in _itemList)
        {
            pc.Inventory.Model.PushItem(item.Item, item.Count);
        }
    }
}