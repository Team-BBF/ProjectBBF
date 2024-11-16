using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/Hoe", fileName = "Bav_Item_Hoe")]
public class PIBHoe : PIBTwoStep
{
    protected override async UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        if (itemData && itemData.Info.Contains(ToolType.Hoe))
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Tool_Using_Hoe");
            AnimateLookAt(playerController, AnimationActorKey.Action.Hoe, true);
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
            .Bind<IBODestoryTile>(x => OnDestroyTile(x, playerController))
            .Bind<IBOCultivateTile>(x => OnCultivateTile(x, playerController))
            .Execute(interaction.ContractInfo, out bool _);

        return ActionResult.Continue;
    }

    protected override UniTask EndAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        return UniTask.CompletedTask;
    }

    private bool OnCultivateTile(IBOCultivateTile action, PlayerController pc)
    {
        var targetPos = pc.Coordinate.GetFront();
        ItemData data = pc.Inventory.CurrentItemData;

        if (data is null) return false;

        bool success = false;

        if (data.Info.Contains(ToolType.Hoe))
        {
            success = action.TryCultivateTile(targetPos, null);
        }

        pc.Inventory.Refresh();

        if (success)
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Digging");
        }

        return success;
    }

    private bool OnDestroyTile(IBODestoryTile action, PlayerController pc)
    {
        var targetPos = pc.Coordinate.GetFront();
        var data = pc.Inventory.CurrentItemData;

        if (data is null) return false;
        var list = action.Destory(targetPos, data.Info);

        if (list is null) return false;

        list.ForEach(item =>
        {
            // 아이템 획득에 실패하면 필드에 아이템 드랍
            if (pc.Inventory.Model.PushItem(item, 1) is not 0)
            {
                // TODO: 레거시
                //FieldItem.Create(new FieldItem.FieldItemInitParameter()
                //{
                //    ItemData = item,
                //    Position = _controller.transform.position
                //});
            }
        });
        
        AudioManager.Instance.PlayOneShot("Player", "Player_Digging");

        if (list.Any())
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Harvest");
        }

        pc.Inventory.Refresh();
        return true;
    }
}