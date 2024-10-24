using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.Serialization;


public class PlayerInteracter : MonoBehaviour, IPlayerStrategy
{
    private PlayerController _controller;
    private ActorVisual _visual;
    private PlayerBlackboard _blackboard;
    private PlayerMove _move;
    private PlayerCoordinate _coordinate;

    private bool _isAniPrevEventRaised = false;
    private bool _isAniNextEventRaised = false;

    public void Init(PlayerController controller)
    {
        _controller = controller;
        _visual = controller.VisualStrategy;
        _move = controller.MoveStrategy;
        _coordinate = controller.Coordinate;
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

    public void OnAnimationBeginEvent(string key)
    {
        if (key == "Begin")
        {
            _isAniPrevEventRaised = false;
        }
        else if (key == "End")
        {
            _isAniNextEventRaised = false;
        }
    }

    public void OnAnimationEndEvent(string key)
    {
        if (key == "Begin")
        {
            _isAniPrevEventRaised = true;
        }
        else if (key == "End")
        {
            _isAniNextEventRaised = true;
        }
    }
    
    public async UniTask<bool> OnToolAction()
    {
        if (_blackboard.IsInteractionStopped) return false;
        
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
                _move.IsStopped = true;
                _blackboard.Energy--;

                _isAniPrevEventRaised = false;
                _isAniNextEventRaised = false;
                
                Vector2 dir = _coordinate.GetFrontPureDir();
                _visual.LookAt(dir, currentData.ActionAnimationType);
                
                PlayAudio(currentData, "Use");
            }
            else
            {
                return false;
            }

            bool success = false;

            if (currentData.UsePrevWait > 0f)
            {
                await UniTask.Delay((int)(currentData.UsePrevWait * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update,
                    this.GetCancellationTokenOnDestroy());
            }
            

            var interaction = FindCloserObject();
            if (interaction is null)
            {
                success = false;
                goto RE;
            }

            if (Farmland(interaction))
            {
                success = true;
                goto RE;
            }
            if(Pickaxe(interaction))
            {
                success = true;
                goto RE;
            }
            
            RE:
            

            if (success)
            {
                PlayAudio(currentData, "Use_Success");
            }
            else
            {
                PlayAudio(currentData, "Use_Fail");
            }


            if (currentData.UseAndWait > 0f)
            {
                await UniTask.Delay((int)(currentData.UseAndWait * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update,
                    this.GetCancellationTokenOnDestroy());
            }
            
            _move.IsStopped = false;
            

            return success;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
            _move.IsStopped = false;
            return false;
        }
        
    }

    private void PlayAudio(ItemData itemData, string usingKey)
    {
        if (itemData == false) return;

        if (itemData.UseActionUsingActionAudioInfos is null) return;
        
        foreach (var info in itemData.UseActionUsingActionAudioInfos)
        {
            if (info.HasAudio(usingKey))
            {
                AudioManager.Instance.PlayOneShot(info.MixerGroupKey, info.AudioKey);
            }
        }
    }

    public async UniTask<bool> OnCollectAction()
    {
        if (_blackboard.IsInteractionStopped) return false;
        
        try
        {
            var interaction = CloserObject;
            if (interaction == false) return false;

            CollisionInteractionUtil
                .CreateSelectState()
                .Bind<IBOCollectPlant>(CollectPlant)
                .Bind<IBOCollect>(CollectObject)
                .Bind<IBACollect>(CollectObject)
                .Bind<IBAInteractionTrigger>(InteractTrigger)
                .Execute(interaction.ContractInfo, out bool executedAny);

            if (executedAny)
            {
                _move.ResetVelocity();
                //Vector2 dir = _coordinate.GetFrontPureDir();

                Vector2 dir = (interaction.transform.position - _controller.transform.position).normalized;
                _visual.LookAt(dir, AnimationActorKey.Action.Collect);
                await UniTask.Delay(500, DelayType.DeltaTime, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
            
            return executedAny;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
            return false;
        }
    }
    public async UniTask<bool> OnActivateAction()
    {
        if (_blackboard.IsInteractionStopped) return false;
        
        try
        {
            var interaction = CloserObject;
            if (interaction == false) return false;

            CollisionInteractionUtil
                .CreateSelectState()
                .Bind<IBAInteractionTrigger>(ActivateTrigger)
                .Execute(interaction.ContractInfo, out bool executedAny);

            if (executedAny)
            {
                _move.ResetVelocity();
                _visual.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Collect, AnimationActorKey.Direction.Down));
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
    
    private bool InteractTrigger(IBAInteractionTrigger arg)
    {
        _controller.StateHandler.TranslateState("EndOfInteraction");
        bool success = arg.Interact(_controller.Interaction);
        return success;
    }
    private bool ActivateTrigger(IBAInteractionTrigger arg)
    {
        _controller.StateHandler.TranslateState("EndOfInteraction");
        bool success = arg.Activate(_controller.Interaction);
        return success;
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

        if (success)
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Tool_Using_Fertilizer");
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

        if (success)
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Digging");
        }

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
            if (_controller.Inventory.Model.PushItem(item, 1) is not 0)
            {
                // TODO: 레거시
                //FieldItem.Create(new FieldItem.FieldItemInitParameter()
                //{
                //    ItemData = item,
                //    Position = _controller.transform.position
                //});
            }
        });

        if (list.Any())
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Harvest");
        }

        _controller.Inventory.Refresh();
        return true;
    }

    private bool CollectPlant(IBOCollectPlant action)
    {
        var targetPos = _controller.Coordinate.GetFront();
        var data = _controller.Inventory.CurrentItemData;

        List<ItemData> items = new List<ItemData>(2);

        if (action.Collect(targetPos, items) is false) return false;
        
        
        AudioManager.Instance.PlayOneShot("Player", "Player_Harvest");

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
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Action.Idle);
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
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Action.Idle);
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
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Action.Idle);
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
            actor.Visual.LookAt(transform.position - actor.transform.position, AnimationActorKey.Action.Idle);
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

    private List<CollisionInteractionMono> _closerObjects = new(5);
    public CollisionInteractionMono CloserObject { get; private set; }
    public event Action<CollisionInteractionMono> OnChangedCloserObject;

    private void Update()
    {
        float minDis = Mathf.Infinity;
        CollisionInteractionMono minObj = null;

        int nullCount = 0;
        
        foreach (CollisionInteractionMono obj in _closerObjects)
        {
            if (obj == false)
            {
                nullCount++;
                continue;
            }
            
            float dis = ((Vector2)(obj.transform.position - _controller.transform.position)).sqrMagnitude;
            
            if ( dis< minDis)
            {
                minDis = dis;
                minObj = obj;
            }
        }

        if (CloserObject != minObj)
        {
            CloserObject = minObj;
            OnChangedCloserObject?.Invoke(CloserObject);
        }

        if (nullCount >= 10)
        {
            _closerObjects.RemoveAll(x=>x == false);
        }
    }

    public void AddCloserObject(CollisionInteractionMono interaction)
    {
        _closerObjects.Add(interaction);
    }
    public void RemoveCloserObject(CollisionInteractionMono interaction)
    {
        _closerObjects.Remove(interaction);
    }

    public bool ContainsCloserObject(CollisionInteractionMono interaction)
    {
        return _closerObjects.Contains(interaction);
    }
}