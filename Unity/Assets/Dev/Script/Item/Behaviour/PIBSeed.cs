using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/Seed",  fileName = "Bav_Item_Seed")]
public class PIBSeed : PIBTwoStep
{
    protected override async UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        if (itemData && itemData.Info.Contains(ToolType.Seed))
        {
            var interaction = playerController.Interactor.FindCloserObject();
            if (interaction == false) return ActionResult.Continue;
            
            var targetPos = playerController.Coordinate.GetFront();
            if (interaction.TryGetContractInfo(out ObjectContractInfo info) &&
                info.TryGetBehaviour(out IBOPlantTile platTile) &&
                platTile.CanPlant(targetPos))
            {
                AnimateLookAt(playerController, AnimationActorKey.Action.Plant, true);
                return ActionResult.Continue;
            }
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
            .Bind<IBOPlantTile>(x => PlantTile(x, playerController))
            .Execute(interaction.ContractInfo, out bool _);
        
        return ActionResult.Continue;
    }

    protected override UniTask EndAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        return UniTask.CompletedTask;
    }
    
    public bool PlantTile(IBOPlantTile action, PlayerController pc)
    {
        var targetPos = pc.Coordinate.GetFront();
        ItemData data = pc.Inventory.CurrentItemData;
        IInventorySlot slot = pc.Inventory.CurrentItemSlot;

        bool success = false;

        if (data is PlantItemData grownData && action.Plant(targetPos, grownData.Definition))
        {
            success = slot.TryAdd(-1, true) is SlotStatus.Success;
        }

        if (success)
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Tool_Using_Fertilizer");
        }

        pc.Inventory.Refresh();

        return success;
    }
}