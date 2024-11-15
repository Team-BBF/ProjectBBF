using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

//[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/Sample name",  fileName = "Bav_Item_Sample")]
public abstract class PlayerItemBehaviour : ScriptableObject
{
    public abstract UniTask DoAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default);

    protected static void AnimateLookAt(PlayerController playerController, AnimationActorKey.Action ani, bool force = false)
    {
        ActorVisual visual = playerController.VisualStrategy;

        Vector2 clickPoint = Camera.main.ScreenToWorldPoint(InputManager.Map.Player.Look.ReadValue<Vector2>());
        Vector2 dir = clickPoint - (Vector2)playerController.transform.position;
        
        visual.SetAction(ani, dir);
    }
}