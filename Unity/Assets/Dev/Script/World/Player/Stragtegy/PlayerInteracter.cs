using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DS.Core;
using JetBrains.Annotations;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerInteracter : MonoBehaviour, IPlayerStrategy
{
    private PlayerController _controller;
    private ActorVisual _visual;
    private PlayerBlackboard _blackboard;
    private PlayerMove _move;

    public void Init(PlayerController controller)
    {
        _controller = controller;
        _visual = controller.VisualStrategy;
        _move = controller.MoveStrategy;
        _blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
    }

    public bool MainInventoryVisible
    {
        get => _controller.Inventory.MainInvVisible;
        set => _controller.Inventory.MainInvVisible = value;
    }

    public bool QuickInventoryVisible
    {
        get => _controller.Inventory.QuickInvVisible;
        set => _controller.Inventory.QuickInvVisible = value;
    }
    
    public async UniTask<bool> OnToolAction()
    {
        try
        {
            ItemData currentData = _controller.Inventory.CurrentItemData;

            if (currentData && (
                    currentData.Info.Contains(ToolType.Hoe) ||
                    currentData.Info.Contains(ToolType.WaterSpray) ||
                    currentData.Info.Contains(ToolType.Fertilizer) ||
                    currentData.Info.Contains(ToolType.Seed) ||
                    currentData.Info.Contains(ToolType.Pickaxe)
                ))
            {
                if (_blackboard.Energy < 1)
                {
                    return false;
                }

                _move.ResetVelocity();
                _blackboard.Energy--;

                _visual.ChangeClip(currentData.ActionAnimationAniHash);
            }
            else
            {
                return false;
            }

            var interaction = FindCloserObject();
            if (interaction is null)
            {
                await UniTask.Delay(300, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                return false;
            }

            if (Farmland(interaction))
            {
                await UniTask.Delay(300, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                return true;
            }
            if(currentData.Info.Contains(ToolType.Pickaxe) && Pickaxe(interaction))
            {
                await UniTask.Delay(300, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                return true;
            }

            await UniTask.Delay(300, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            return false;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
            return false;
        }
        
        return true;
    }

    public async UniTask<bool> OnCollectAction()
    {
        try
        {
            var interaction = FindCloserObject();
            if (interaction is null) return false;

            CollisionInteractionUtil
                .CreateSelectState()
                .Bind<IBOCollectPlant>(CollectPlant)
                .Bind<IBOCollect>(CollectObject)
                .Bind<IBACollect>(CollectObject)
                .Execute(interaction.ContractInfo, out bool executedAny);

            if (executedAny)
            {
                _move.ResetVelocity();
                _visual.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Collect));
                await UniTask.Delay(300, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            
            return executedAny;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
            return false;
        }
    }

    private bool Farmland(CollisionInteractionMono interaction)
    {
        CollisionInteractionUtil
            .CreateSelectState()
            .Bind<IBODestoryTile>(DestroyTile)
            .Bind<IBOFertilizerTile>(FertilizerTile)
            .Bind<IBOSprinkleWaterTile>(SprinkleWater)
            .Bind<IBOCultivateTile>(CultivateTile)
            .Bind<IBOPlantTile>(PlantTile)
            .Execute(interaction.ContractInfo, out bool executedAny);

        return executedAny;
    }

    private bool Pickaxe(CollisionInteractionMono interaction)
    {
        CollisionInteractionUtil
            .CreateSelectState()
            .Bind<IBOCollectPlant>(CollectPlant)
            .Bind<IBOCollectTool>(CollectObject)
            .Bind<IBACollectTool>(CollectObject)
            .Execute(interaction.ContractInfo, out bool executedAny);
            
        return executedAny;
    }


    #region Actor

    #endregion

    #region Object

    public bool PlantTile(IBOPlantTile action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        ItemData data = _controller.Inventory.CurrentItemData;
        IInventorySlot slot = _controller.Inventory.CurrentItemSlot;

        bool success = false;

        if (data is PlantItemData grownData && action.Plant(targetPos, grownData.Definition))
        {
            success = slot.TryAdd(-1, true) is SlotStatus.Success;
        }

        _controller.Inventory.Refresh();

        return success;
    }

    private bool FertilizerTile(IBOFertilizerTile action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        ItemData data = _controller.Inventory.CurrentItemData;
        IInventorySlot slot = _controller.Inventory.CurrentItemSlot;

        if (data is null) return false;
        if (slot is null) return false;

        bool success = false;

        if (data.Info.Contains(ToolType.Fertilizer))
        {
            if (slot.Data is FertilizerItemData fertilizerItem)
            {
                success = action.PlantFertilizer(targetPos, fertilizerItem.TargetTile);

                if (success)
                    slot.TryAdd(-1, true);
            }
        }

        _controller.Inventory.Refresh();

        return success;
    }

    public bool CultivateTile(IBOCultivateTile action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        ItemData data = _controller.Inventory.CurrentItemData;

        if (data is null) return false;

        bool success = false;

        if (data.Info.Contains(ToolType.Hoe))
        {
            success = action.TryCultivateTile(targetPos, null);
        }

        _controller.Inventory.Refresh();

        return success;
    }

    private bool SprinkleWater(IBOSprinkleWaterTile action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        ItemData data = _controller.Inventory.CurrentItemData;

        if (data is null) return false;

        bool success = false;

        if (data.Info.Contains(ToolType.WaterSpray))
        {
            success = action.SprinkleWater(targetPos);
        }

        return success;
    }

    public bool DestroyTile(IBODestoryTile destoryTile)
    {
        var targetPos = _controller.Coordinate.GetFront();
        var data = _controller.Inventory.CurrentItemData;

        if (data is null) return false;
        var list = destoryTile.Destory(targetPos, data.Info);

        if (list is null) return false;

        list.ForEach(item =>
        {
            // 아이템 획득에 실패하면 필드에 아이템 드랍
            if (_controller.Inventory.Model.PushItem(item, 1) == false)
            {
                FieldItem.Create(new FieldItem.FieldItemInitParameter()
                {
                    ItemData = item,
                    Position = _controller.transform.position
                });
            }
        });

        _controller.Inventory.Refresh();
        return true;
    }

    private bool CollectPlant(IBOCollectPlant action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        var data = _controller.Inventory.CurrentItemData;

        List<ItemData> items = new List<ItemData>(2);

        if (action.Collect(targetPos, items) is false) return false;

        foreach (var item in items)
        {
            _controller.Inventory.Model.PushItem(item, 1);
        }

        _controller.Inventory.Refresh();

        return true;
    }

    private bool CollectObject(IBOCollect action)
    {
        var list = action.Collect();
        if (list is null) return false;

        if (action.Interaction.Owner is Actor actor)
        {
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Movement.Idle);
            actor.TransitionHandler.TranslateState("ToWait");
        }

        ColectObject(list);

        return true;
    }

    private bool CollectObject(IBACollect action)
    {
        var list = action.Collect();
        if (list is null) return false;


        if (action.Interaction.Owner is Actor actor)
        {
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Movement.Idle);
            actor.TransitionHandler.TranslateState("ToWait");
        }
        
        ColectObject(list);

        return true;
    }

    private bool CollectObject(IBACollectTool action)
    {
        ItemData currentData = _controller.Inventory.CurrentItemData;
        if(currentData == false) return false;

        bool flag = false;
        foreach (ToolRequireSet toolSet in currentData.Info.Sets)
        {
            if (toolSet is null) continue;
            
            if(action.CanCollect(toolSet))
            {
                flag = true;
            }
        }

        if (flag is false) return false;
        
        var list = action.Collect();
        if (list is null) return false;


        if (action.Interaction.Owner is Actor actor)
        {
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Movement.Idle);
            actor.TransitionHandler.TranslateState("ToWait");
        }
        
        ColectObject(list);

        return true;
    }

    private bool CollectObject(IBOCollectTool action)
    {
        ItemData currentData = _controller.Inventory.CurrentItemData;
        if(currentData == false) return false;

        bool flag = false;
        foreach (ToolRequireSet toolSet in currentData.Info.Sets)
        {
            if (toolSet is null) continue;
            
            if(action.CanCollect(toolSet))
            {
                flag = true;
            }
        }

        if (flag is false) return false;
        
        var list = action.Collect();
        if (list is null) return false;


        if (action.Interaction.Owner is Actor actor)
        {
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Movement.Idle);
            actor.TransitionHandler.TranslateState("ToWait");
        }
        
        ColectObject(list);

        return true;
    }

    private void ColectObject(List<ItemData> items)
    {
        if (items is null) return;

        foreach (ItemData item in items)
        {
            _controller.Inventory.Model.PushItem(item, 1);
        }

        _controller.Inventory.Refresh();
    }

    #endregion

    public CollisionInteractionMono FindCloserObject()
    {
        var targetPos = _controller.Coordinate.GetFront();
        var colliders =
            Physics2D.OverlapCircleAll(targetPos, _controller.InteractionRadius, ~LayerMask.GetMask("Player"));

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