


using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;

public class MapInventoryClear : MonoBehaviour
{
    public void Clear(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        
        
        var ahirBox = PersistenceManager.Instance.LoadOrCreate<AhirBoxPersistenceObject>("AhirBox_Ch_2");
        
        pc.Inventory.Model.Clear();
        
        if (string.IsNullOrEmpty(ahirBox.ItemKey) is false && DataManager.Instance)
        {
            if (DataManager.Instance.GetResolver<IItemDataResolver>()
                .TryGetData(ahirBox.ItemKey, out var passedItemData))
            {
                pc.Inventory.Model.PushItem(passedItemData, 1);
            }
        }
    }
}