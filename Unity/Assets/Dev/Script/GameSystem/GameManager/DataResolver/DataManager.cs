

using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Singleton;
using UnityEngine;

[Singleton(ESingletonType.Global)]
public partial class DataManager : MonoBehaviourSingleton<DataManager>
{
    private Dictionary<Type, IBaseDataResolver> _resolvers;
    public override void PostInitialize()
    {
        _resolvers = new(2);
        
        PartialBind();
    }

    public T GetResolver<T>()
        where T : class, IBaseDataResolver
    {
        if (_resolvers.TryGetValue(typeof(T), out IBaseDataResolver value) is false)
        {
            Debug.LogError($"Resolver를 찾을 수 없음. T({typeof(T)})");
            return null;
        }

        return value as T;
    }

    private DataManager Bind<TAbstract, TResolver>()
        where TAbstract : IBaseDataResolver
        where TResolver : TAbstract, new()
    {
        if (_resolvers.ContainsKey(typeof(TAbstract)))
        {
            Debug.LogError($"이미 등록된 Abstract({typeof(TAbstract)}, Resolver({typeof(TResolver)}))");
            return this;
        }
        
        var v = new TResolver();
        v.Init();
        
        _resolvers.Add(typeof(TAbstract), v);

        return this;
    }

    public override void PostRelease()
    {
        foreach (IBaseDataResolver dataResolver in _resolvers.Values)
        {
            dataResolver.Release();
        }
    }
}

public interface IBaseDataResolver
{
    public bool IsInit();
    public void Init();
    public void Release();
}
public interface IDataResolver<T> : IBaseDataResolver
{
    public bool TryGetData(string key, out T value);
}

