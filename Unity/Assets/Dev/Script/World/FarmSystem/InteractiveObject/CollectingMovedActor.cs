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
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CollectingMovedActor : ActorComponent
{
    
    [field: SerializeField, InitializationField]
    private CollectingObjectData _collectingData;

    [SerializeField, Header("nullable")] [CanBeNull] private ParticlePlayer _particle;

    [SerializeField] private List<ESOVoid> _collectAndRaisingEvents;
    [SerializeField] private UnityEvent<CollectState> _onChangedCollectingState;

    [SerializeField] private string _collectedPlayingAudioGroupKey;
    [SerializeField] private string _collectedPlayingAudioKey;
    
    [field: SerializeField, InitializationField, MustBeAssigned]
    private List<ESOGameTimeEvent> _refillEvents;
    
    //[SerializeField] private GameObject  

    private PatrolPointPath _currentPath;
    private Actor _masterActor;
    public bool CanCollect => CollectCount < _collectingData.MaxCollectCount;

    public CollectingObjectData CollectingData => _collectingData;
    public CollisionInteraction Interaction => _masterActor.Interaction;

    public UnityEvent<CollectState> OnChangedCollectingState => _onChangedCollectingState;
    private int _collectCount;

    public int CollectCount
    {
        get => _collectCount;
        set
        {
            _collectCount = value;

            if (CanCollect)
            {
                Refill(false);
            }
            else
            {
                ChangeCollected();
            }
        }
    }

    private class ToolBehaviour : IBOInteractiveTool
    {
        public CollectingMovedActor _actorCom;
        public CollisionInteraction Interaction { get; set; }
        public bool IsVaildTool(ToolRequireSet toolSet)
        {
            return ToolTypeUtil.Contains(_actorCom._collectingData.RequireSet, toolSet);
        }

        public void UpdateInteract(CollisionInteractionMono caller)
        {
            if (caller.Owner is not PlayerController pc) return;
            if (pc.Inventory.CurrentItemData == null) return;

            foreach (ToolRequireSet toolSet in pc.Inventory.CurrentItemData.Info.Sets)
            {
                var list = _actorCom.Collect();

                if (list is null) return;

                foreach (ItemData item in list)
                {
                    pc.Inventory.Model.PushItem(item, 1);
                }
                    
                return;
            }
        }
    }
    private class CollectBehaviour : IBOInteractiveSingle
    {
        public CollectingMovedActor _actorCom;
        public CollisionInteraction Interaction { get; set; }
        public void UpdateInteract(CollisionInteractionMono caller)
        {
            if (caller.Owner is not PlayerController pc) return;
            if (pc.Inventory.CurrentItemData == null) return;

            var list = _actorCom.Collect();

            if (list is null) return;

            foreach (ItemData item in list)
            {
                pc.Inventory.Model.PushItem(item, 1);
            }
        }
    }

    public IObjectBehaviour CreateCollectProxyOrNull()
    {
        if (CollectingData)
        {
            if (CollectingData.OnlyTool)
            {
                return new ToolBehaviour()
                {
                    Interaction = _masterActor.Interaction,
                    _actorCom = this
                };
            }
            else
            {
                return new CollectBehaviour()
                {
                    Interaction = _masterActor.Interaction,
                    _actorCom = this
                };
            }
        }

        return null;
    }
    
    public void Init(Actor actor)
    {
        _masterActor = actor;
        foreach (var refillEvent in _refillEvents)
        {
            if (refillEvent == false) continue;
            refillEvent.OnSignal += Refill;
        }
        
        var obj = PersistenceManager.Instance.LoadOrCreate<CollectPersistenceObject>(_collectingData.PersistenceKey);

        PersistenceManager.Instance.OnGameDataLoaded += OnLoaded;
        obj.Actor = this;
        
        if (obj.Saved is false)
        {
            obj.Saved = true;
        }
        else
        {
            obj.Load(this);
        }
    }

    private void OnLoaded(PersistenceManager inst)
    {
        if (this == false)
        {
            inst.OnGameDataLoaded -= OnLoaded;
            return;
        }
        
        var obj = PersistenceManager.Instance.LoadOrCreate<CollectPersistenceObject>(_collectingData.PersistenceKey);
        obj.Actor = this;
        obj.Load(this);
    }

    private void OnDestroy()
    {
        foreach (var refillEvent in _refillEvents)
        {
            if (refillEvent == false) continue;
            refillEvent.OnSignal -= Refill;
        }

        if (PersistenceManager.Instance)
        {
            PersistenceManager.Instance.OnGameDataLoaded -= OnLoaded;
            var obj = PersistenceManager.Instance.LoadOrCreate<CollectPersistenceObject>(_collectingData.PersistenceKey);

            obj.Actor = this;
            obj.Save(this);
        }
    }

    public void Refill(GameTime obj = default)
    {
        _collectCount = 0;
        _onChangedCollectingState?.Invoke(CollectState.Normal);
    }
    public void Refill(bool isSetCountZero)
    {
        if (isSetCountZero)
        {
            _collectCount = 0;
        }

        _onChangedCollectingState?.Invoke(CollectState.Normal);
    }

    public void ChangeCollected()
    {
        _collectCount = CollectingData.MaxCollectCount;
        _onChangedCollectingState?.Invoke(CollectState.Collected);
    }
    
    public List<ItemData> Collect()
    {
        
        if (_collectingData == false) return null;
        if (CollectCount >= CollectingData.MaxCollectCount) return null;
            
        var list = new List<ItemData>();
        if (CanCollect is false) return null;

        _collectCount++;

        if (_particle)
        {
            _particle.Play();
        }
        
        if (string.IsNullOrEmpty(_collectedPlayingAudioGroupKey) is false && 
            string.IsNullOrEmpty(_collectedPlayingAudioKey) is false)
        {
            string groupKey = _collectedPlayingAudioGroupKey.Trim();
            string audiokey = _collectedPlayingAudioKey.Trim();
            AudioManager.Instance.PlayOneShot(groupKey, audiokey);
        }

        foreach (var item in _collectingData.DropItems)
        {
            for (int i = 0; i < item.Count; i++)
            {
                list.Add(item.Data);
            }
        }
        

        foreach (var eso in _collectAndRaisingEvents)
        {
            if (eso)
            {
                eso.Raise();
            }
        }

        if (_collectCount >= CollectingData.MaxCollectCount)
        {
            _onChangedCollectingState?.Invoke(CollectState.Collected);
        }

        return list;
    }
}