using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Event;
using JetBrains.Annotations;
using UnityEngine;



public class ObjectContractInfo : BaseContractT<IObjectBehaviour, ObjectContractInfo>
{
    
}
public class ActorContractInfo : BaseContractT<IActorBehaviour, ActorContractInfo>
{
    
}
public class ClickContractInfo: BaseContractInfo
{
    public int MouseButtonNumber;
    public EClickContractType clickType;
    public override IBaseBehaviour GetBehaviourOrNull(Type type)
    {
        return null;
    }
}
public class ListeningContractInfo: BaseContractT<INoBehaviour, ListeningContractInfo>
{
    
}

public enum EClickContractType
{
    OneClick,
    DoubleClick,
    Pressed
}



public abstract class BaseContractInfo
{

    internal CollisionInteraction _interaction;
    public CollisionInteraction Interaction => _interaction;
    public bool IsDestroyed => Check();
    
    protected Func<bool> _destroyChecker;
    private bool Check()
    {
        if (_destroyChecker == null)
            return true;

        bool flag = _destroyChecker();
        if (flag) _destroyChecker = null;

        return !flag;
    }

    public abstract IBaseBehaviour GetBehaviourOrNull(Type type);
}

public abstract class BaseContractT<TBASE, TCLASS> : BaseContractInfo
    where TBASE : IBaseBehaviour 
    where TCLASS : BaseContractT<TBASE, TCLASS>, new()
{

    private Dictionary<Type, IBaseBehaviour> _table = new();

    protected BaseContractT()
    {
    }

    public static TCLASS Create(Func<bool> destroyChecker) =>
        new() { _destroyChecker = destroyChecker };

    public TCLASS AddBehaivour<T>(TBASE behaviour) where T : class, TBASE
    {
        if(behaviour is null) throw new ArgumentNullException("argument is null");
        if (behaviour is not T) throw new ArgumentException($"[{behaviour.GetType().Name}] is not [{typeof(T).Name}].");

        if (!_table.TryAdd(typeof(T), behaviour))
        {
            throw new ArgumentException($"[{typeof(T).Name}] is already exist");
        }
        
        return this as TCLASS;
    }
    
    public T GetBehaviourOrNull<T>() where T : class, TBASE
    {
        if (_table.TryGetValue(typeof(T), out var v))
        {
            return v as T;
        }
        
        return null;
    }

    public bool TryGetBehaviour<T>(out T value) where T : class, TBASE
    {
        value = GetBehaviourOrNull<T>();

        return value != null;
    }

    public override IBaseBehaviour GetBehaviourOrNull(Type type)
    {
        return _table.GetValueOrDefault(type);
    }
}