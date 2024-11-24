using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/ToolCollect ",  fileName = "Bav_Item_ToolCollect")]
public class PIBToolCollect : PIBTwoStep
{
    private CollisionInteractionMono _target;
    
    private bool CollectObject(IBOInteractiveTool action, PlayerController pc)
    {
        var itemData = pc.Inventory.CurrentItemData;
        if (itemData == false) return false;

        foreach (ToolRequireSet set in itemData.Info.Sets)
        {
            if (action.IsVaildTool(set))
            {
                action.UpdateInteract(pc.Interaction);
                return true;
            }
        }


        return false;
    }
    protected override async UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        _target = playerController.Interactor.FindCloserObject();
        if (_target == false) return ActionResult.Break;
        
        return ActionResult.Continue;
    }

    protected override async UniTask<ActionResult> PostAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        if(_target == false) return ActionResult.Break;
        
        CollisionInteractionUtil
            .CreateSelectState()
            .Bind<IBOInteractiveTool>(x => CollectObject(x, playerController))
            .Execute(_target.ContractInfo, out bool _);

        return ActionResult.Continue;
    }

    protected override UniTask EndAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        return UniTask.CompletedTask;
    }
}