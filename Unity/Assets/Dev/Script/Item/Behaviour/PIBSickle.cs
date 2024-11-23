using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/Sickle", fileName = "Bav_Item_Sickle")]
public class PIBSickle : PIBTwoStep
{
    private Vector2 _targetPos;
    
    protected override async UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        if (itemData && itemData.Info.Contains(ToolType.Sickle))
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Tool_Using_Sickle");
            AnimateLookAt(playerController, AnimationActorKey.Action.Sickle, true);
            _targetPos = playerController.Interactor.IndicatedPosition;
            
            return ActionResult.Continue;
        }


        return ActionResult.Break;
    }

    protected override async UniTask<ActionResult> PostAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        var interaction = playerController.Interactor.FindCloserObject();
        if (interaction == false) return ActionResult.Continue;
        
        CollisionInteractionUtil
            .CreateSelectState()
            .Bind<IBOCollectPlant>(x => OnCollect(x, playerController))
            .Execute(interaction.ContractInfo, out bool _);

        return ActionResult.Continue;
    }

    protected override UniTask EndAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        return UniTask.CompletedTask;
    }

    private bool OnCollect(IBOCollectPlant action, PlayerController pc)
    {
        ItemData data = pc.Inventory.CurrentItemData;

        if (data is null) return false;

        bool success = false;

        List<ItemData> list = new(2);

        if (action.Collect(_targetPos, list))
        {
            success = true;
            list.ForEach(x => pc.Inventory.Model.PushItem(x, 1));
        }
        
        if (list.Any())
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Harvest");
        }

        pc.Inventory.Refresh();

        if (success)
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Digging");
        }

        return success;
    }
}