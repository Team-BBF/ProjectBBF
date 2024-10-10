using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using JetBrains.Annotations;
using ProjectBBF.Event;
using MyBox;
using UnityEngine;

[RequireComponent(typeof(CollisionInteraction))]
public class CollectingMovedActor : ActorComponent
{
    [field: SerializeField, InitializationField, MustBeAssigned]
    private CollectingObjectData _collectingData;

    [field: SerializeField, InitializationField, MustBeAssigned]
    private List<ESOGameTimeEvent> _refillEvents;

    private int _collectCount;
    private PatrolPointPath _currentPath;
    private Actor _masterActor;
    public bool CanCollect => _collectCount < _collectingData.MaxCollectCount;

    public CollectingObjectData CollectingData => _collectingData;
    public CollisionInteraction Interaction => _masterActor.Interaction;

    private class ToolBehaviour : IBACollectTool
    {
        public CollectingMovedActor _actorCom;
        public CollisionInteraction Interaction { get; set; }
        public bool CanCollect(ToolRequireSet toolSet)
        {
            return ToolTypeUtil.Contains(_actorCom._collectingData.RequireSet, toolSet);
        }

        public List<ItemData> Collect()
        {
            var list = new List<ItemData>();

            if (_actorCom.CanCollect is false)
            {
                return null;
            }

            _actorCom._collectCount++;
        
            foreach (var item in _actorCom._collectingData.DropItems)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    list.Add(item.Data);
                }
            }
        

            return list;
        }
    }
    private class CollectBehaviour : IBACollect
    {
        public CollectingMovedActor _actorCom;
        public CollisionInteraction Interaction { get; set; }


        public List<ItemData> Collect()
        {
            var list = new List<ItemData>();

            if (_actorCom.CanCollect is false)
            {
                return null;
            }

            _actorCom._collectCount++;
        
            foreach (var item in _actorCom._collectingData.DropItems)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    list.Add(item.Data);
                }
            }
        

            return list;
        }
    }
    
    public override void Init(Actor actor)
    {
        _masterActor = actor;
        var info = actor.Interaction.ContractInfo as ActorContractInfo;

        if (_collectingData.OnlyTool)
        {
            info!.AddBehaivour<IBACollectTool>(new ToolBehaviour()
            {
                Interaction = _masterActor.Interaction,
                _actorCom = this
            });
        }
        else
        {
            info!.AddBehaivour<IBACollect>(new CollectBehaviour()
            {
                Interaction = _masterActor.Interaction,
                _actorCom = this
            });
        }

        foreach (var refillEvent in _refillEvents)
        {
            if (refillEvent == false) continue;
            refillEvent.OnSignal += Refill;
        }
    }
    private void OnDestroy()
    {
        foreach (var refillEvent in _refillEvents)
        {
            if (refillEvent == false) continue;
            refillEvent.OnSignal -= Refill;
        }
    }

    private void Refill(GameTime obj = default)
    {
        _collectCount = 0;
    }
}