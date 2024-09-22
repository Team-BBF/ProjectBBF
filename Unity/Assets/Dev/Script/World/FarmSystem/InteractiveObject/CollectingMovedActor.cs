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
public class CollectingMovedActor : ActorComponent, IBACollect
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
    
    public override void Init(Actor actor)
    {
        (actor.Interaction.ContractInfo as ActorContractInfo)!.AddBehaivour<IBACollect>(this);

        foreach (var refillEvent in _refillEvents)
        {
            if (refillEvent == false) continue;
            refillEvent.OnSignal += Refill;
        }
        
        _masterActor = actor;
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

    public List<ItemData> Collect()
    {
        var list = new List<ItemData>();

        if (CanCollect is false)
        {
            return null;
        }

        _collectCount++;
        
        foreach (var item in _collectingData.DropItems)
        {
            for (int i = 0; i < item.Count; i++)
            {
                list.Add(item.Data);
            }
        }
        

        return list;
    }
}