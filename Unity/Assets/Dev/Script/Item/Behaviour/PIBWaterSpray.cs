using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/WaterSpray",  fileName = "Bav_Item_WaterSpray")]
public class PIBWaterSpray : PIBTwoStep
{
    protected override async UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        if (itemData && itemData.Info.Contains(ToolType.WaterSpray))
        {
            AnimateLookAt(playerController, AnimationActorKey.Action.WaterSpray);
            return ActionResult.Continue;
        }


        return ActionResult.Break;
    }
    protected override async UniTask<ActionResult> PostAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        var interaction = playerController.Interactor.FindCloserObject();
        if (interaction == false) return ActionResult.Continue;
        
        CollisionInteractionUtil
            .CreateSelectState()
            .Bind<IBOSprinkleWaterTile>(x => SprinkleWater(x, playerController))
            .Execute(interaction.ContractInfo, out bool _);
        
        return ActionResult.Continue;
    }
    
    private bool SprinkleWater(IBOSprinkleWaterTile action, PlayerController pc)
    {
        var targetPos = pc.Coordinate.GetFront();
        ItemData data = pc.Inventory.CurrentItemData;

        if (data is null) return false;

        bool success = false;

        if (data.Info.Contains(ToolType.WaterSpray))
        {
            success = action.SprinkleWater(targetPos);
        }

        return success;
    }
}