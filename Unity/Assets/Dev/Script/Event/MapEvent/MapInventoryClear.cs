


using ProjectBBF.Event;
using UnityEngine;

public class MapInventoryClear : MonoBehaviour
{
    public void Clear(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        
        pc.Inventory.Model.Clear();
    }
}