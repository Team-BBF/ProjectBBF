



using System;
using System.Collections.Generic;
using DS.Core;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.AI;

public class LyllaFavorability : ActorComFavorability
{
    [SerializeField] private DialogueContainer _interruptDialogue;
    [SerializeField] private DialogueContainer _arriveDialogue;
    [SerializeField] private ESOVoid _esoMoveNextLock;
    [SerializeField] private ESOVoid _esoMoveNextUnlock;
    [SerializeField] private ESOVoid _EsoMovePrev;
    [SerializeField] private string _chapterKey;
    
    private const string _PERSISTENCE_KEY = "LyllaFavorability";
    

    private int _index;
    private bool _moveNextLock;
    private LyllaFavorabilityPersistenceObject _persistenceObject;
    private ActorMove _move;

    public override void Init(Actor actor)
    {
        base.Init(actor);
        
        _move = actor.MoveStrategy;

        _persistenceObject = PersistenceManager.Instance.LoadOrCreate<LyllaFavorabilityPersistenceObject>(_PERSISTENCE_KEY);
        _index = 0;
        if (_persistenceObject._indexTable.TryGetValue(_chapterKey, out int index))
        {
            _index = index;
        }
        else
        {
            _persistenceObject._indexTable.Add(_chapterKey, 0);
        }

        _moveNextLock = _persistenceObject.MoveNextLock;

        _esoMoveNextUnlock.OnEventRaised += OnUnlock;
        _esoMoveNextLock.OnEventRaised += OnLock;
        _EsoMovePrev.OnEventRaised += OnMovePrev;
    }

    private void OnMovePrev()
    {
        _index = Mathf.Max(0, _index - 1);
    }

    private void OnDestroy()
    {
        _persistenceObject._indexTable[_chapterKey] = _index;
        _persistenceObject.MoveNextLock = _moveNextLock;
        
        _esoMoveNextUnlock.OnEventRaised -= OnUnlock;
        _esoMoveNextLock.OnEventRaised -= OnLock;
    }

    private void OnUnlock()
    {
        if (_moveNextLock)
        {
            _index++;
        }
        _moveNextLock = false;
    }
    private void OnLock()
    {
        _moveNextLock = true;
    }

    public override DialogueEvent DequeueDialogueEvent()
    {
        IReadOnlyList<FavorabilityEventItem> events =  FavorabilityData.FavorabilityEvent.EventItems;

        if (_move.IsMoving)
        {
            return new DialogueEvent()
            {
                Container = _interruptDialogue,
                Type = DialogueBranchType.Dialogue,
                ProcessorData = ProcessorData
            };
        }

        if (_moveNextLock)
        {
            return new DialogueEvent()
            {
                Container = events[_index].Container,
                Type = DialogueBranchType.Dialogue,
                ProcessorData = ProcessorData
            };
        }

        if (events.Count <= _index || events.Count == 0)
        {
            return new DialogueEvent()
            {
                Container = _arriveDialogue ?? events[^1].Container,
                Type = DialogueBranchType.Dialogue,
                ProcessorData = ProcessorData
            };
        }

        return new DialogueEvent()
        {
            Container = events[_index++].Container,
            Type = DialogueBranchType.Dialogue,
            ProcessorData = ProcessorData
        };
    }

    public override DialogueEvent PeekDialogueEvent()
    {
        int tempIndex = _index;
        var item = DequeueDialogueEvent();
        _index = tempIndex;
        return item; 
    }
}