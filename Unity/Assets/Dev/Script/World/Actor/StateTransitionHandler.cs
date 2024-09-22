using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using Unity.VisualScripting;
using UnityEngine;

public class StateTransitionHandler : MonoBehaviour, IBAStateTransfer
{
    public delegate bool Callback();

    private Dictionary<string, Callback> _callbackTable = new();

    public CollisionInteraction Interaction { get; private set; }

    public void Init(CollisionInteraction interaction)
    {
        Interaction = interaction;
    }
    
    public bool AddHandleCallback(string targetStateKey, Callback callback)
    {
        Debug.Assert(callback is not null);
        Debug.Assert(string.IsNullOrEmpty(targetStateKey) == false);

        return _callbackTable.TryAdd(targetStateKey, callback);
    }

    public bool RemoveHandleCallback(string targetStateKey)
    {
        Debug.Assert(string.IsNullOrEmpty(targetStateKey) == false);

        return _callbackTable.Remove(targetStateKey);
    }

    public void TranslateState(string targetStateKey)
    {
        if(this)
            CustomEvent.Trigger(gameObject, targetStateKey);
    }
}