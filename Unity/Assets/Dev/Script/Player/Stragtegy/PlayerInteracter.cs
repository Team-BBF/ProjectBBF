using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DS.Core;
using JetBrains.Annotations;
using ProjectBBF.Event;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerInteracter : MonoBehaviour, IPlayerStrategy
{
    private PlayerController _controller;

    public void Init(PlayerController controller)
    {
        _controller = controller;
    }

    public async UniTask OnEnter()
    {
        await UniTask.Delay(100, DelayType.DeltaTime, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);

        try
        {
            await Interact();
        }
        catch(Exception e) when(e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }

        await UniTask.Delay(100, DelayType.DeltaTime, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);
    }

    private async UniTask Interact()
    {
        var interaction = FindCloserObject();
        if (interaction is null) return;
        
        Farmland(interaction);
        
        await Dialogue(interaction);
                
        DialogueController.Instance.ResetDialogue();
        _controller.QuickInventory.Visible = true;
    }

    private void Farmland(CollisionInteractionMono interaction)
    {
        CollisionInteractionUtil
            .CreateSelectState()
            .Bind<IBODestoryTile>(DestroyTile)
            .Bind<IBOCultivateTile>(CultivateTile)
            .Bind<IBOPlantTile>(PlantTile)
            .Execute(interaction.ContractInfo);
    }

    private async UniTask Dialogue(CollisionInteractionMono interaction)
    {

        if (interaction.TryGetContractInfo(out ActorContractInfo actorInfo) &&
            actorInfo.TryGetBehaviour(out IBAStateTransfer stateTransfer) &&
            actorInfo.TryGetBehaviour(out IBAFavorablity favorablity) &&
            actorInfo.TryGetBehaviour(out IBANameKey nameKey))
        {
            stateTransfer.TranslateState("TalkingForPlayer");
            _controller.QuickInventory.Visible = false;

            if (actorInfo.Interaction.Owner is Actor actor)
            {
                actor.Visual.LookAt(_controller.transform.position - actor.transform.position, true);
            }
            
            var favorablityContainer = favorablity.FavorablityContainer;
            
            //TODO: test code, delete this
            favorablityContainer.CurrentFavorablity = 3;
            
            var eventItems = favorablityContainer.GetExecutableDialogues();

            var instance = DialogueController.Instance;
            instance.ResetDialogue();
            instance.Visible = true;
            instance.SetDisplayName(nameKey.ActorKey);

            if (ActorDataManager.Instance.Table.Table.TryGetValue(nameKey.ActorKey, out var data))
            {
                instance.SetPortrait(data.ActorKey, data.DefaultPortraitKey);
            }

            var index = await instance.GetBranchResultAsync(
                "대화",
                "선물",
                "떠나기"
            );

            switch (index)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    instance.ResetDialogue();
                    stateTransfer.TranslateState("DailyRoutine");
                    return;
                default:
                    instance.ResetDialogue();
                    stateTransfer.TranslateState("DailyRoutine");
                    return;
            }
            
            
            // 한번만 실행해야하는 이벤트는 블랙리스트에 등록
            foreach (FavorabilityEventItem item in eventItems)
            {
                if (item.Once)
                {
                    favorablityContainer.AddExecutedDialogueGuid(item.Container.Guid);
                }

                DialogueContext context =instance.CreateContext(item.Container);
                
                await UniTask.Yield();
                await context.Next();
                
                while (context.CanNext)
                {
                    await UniTask.Yield();
                    if (InputManager.Actions.DialogueSkip.triggered)
                    {
                        await UniTask.Yield();
                        await context.Next();
                    }
                }

                await UniTask.WaitUntil(() => InputManager.Actions.DialogueSkip.triggered, PlayerLoopTiming.Update,
                    GlobalCancelation.PlayMode);
            }


            stateTransfer.TranslateState("DailyRoutine");
        }
    }

    #region Actor
    #endregion

    #region Object
    public bool PlantTile(IBOPlantTile action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        ItemData data = _controller.QuickInventory.CurrentItemData;
        IInventorySlot slot = _controller.QuickInventory.CurrentItemSlot;
        
        bool success = false;
        
        if (data is GrownItemData grownData && action.Plant(targetPos, grownData.Definition))
        {
            success = slot.TrySetCount(slot.Count - 1, true);
        }
        
        _controller.QuickInventory.ViewRefresh();
        _controller.Inventory.ViewRefresh();
        
        return success;
    }

    public bool CultivateTile(IBOCultivateTile action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        ItemData data = _controller.QuickInventory.CurrentItemData;
        IInventorySlot slot = _controller.QuickInventory.CurrentItemSlot;


        bool success = false;
        
        if (data.Info.Contains(ToolType.Hoe))
        {
            success = action.TryCultivateTile(targetPos, null);
        }
        
        _controller.QuickInventory.ViewRefresh();
        _controller.Inventory.ViewRefresh();
        
        return success;
    }

    public bool DestroyTile(IBODestoryTile destoryTile)
    {
        var targetPos = _controller.Coordinate.GetFront();
        var data = _controller.QuickInventory.CurrentItemData;

        if (data is null) return false;
        var list = destoryTile.Destory(targetPos, data.Info);

        if (list is null) return false;

        list.ForEach(item =>
        {
            // 아이템 획득에 실패하면 필드에 아이템 드랍
            if (_controller.Inventory.PushItem(item, 1) == false)
            {
                FieldItem.Create(new FieldItem.FieldItemInitParameter()
                {
                    ItemData = item,
                    Position = _controller.transform.position
                });
            }
        });

        _controller.Inventory.ViewRefresh();
        _controller.QuickInventory.ViewRefresh();
        return true;
    }
    #endregion

    public CollisionInteractionMono FindCloserObject()
    {
        var targetPos = _controller.Coordinate.GetFront();
        var colliders = Physics2D.OverlapCircleAll(targetPos, _controller.InteractionRadius);

        float minDis = Mathf.Infinity;
        CollisionInteractionMono minInteraction = null;
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out CollisionInteractionMono interaction)
               )
            {
                float dis = (transform.position - col.transform.position).sqrMagnitude;
                if (dis < minDis)
                {
                    minInteraction = interaction;
                    minDis = dis;
                }
            }
        }

        return minInteraction;
    }
}