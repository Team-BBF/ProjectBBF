using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectBBF.Singleton
{
    public interface IScopeObject
    {
        public void Initialize();

        public void Release();
    }
    
    [Singleton(ESingletonType.Global)]
    public class ScopeSingleton : IGeneralSingleton
    {
        private Dictionary<Type, IScopeObject> _registeredTable;

        private AsyncReactiveProperty<IScopeObject> _registerTrigger;
        private CancellationTokenSource _cancellation;
        
        public void Initialize()
        {
            _registeredTable = new();
            _registerTrigger = new(null);
            _cancellation = new();
        }

        public void Release()
        {
            foreach (var value in _registeredTable.Values)
            {
                value.Release();
            }
            
            _registeredTable.Clear();
            _registeredTable = null;

            _registerTrigger = null;
            
            _cancellation?.Cancel();
            _cancellation = null;
        }

        public void Register<T>(IScopeObject scopeObject) where T : class, IScopeObject
        {
            var type = scopeObject.GetType();
            if (_registeredTable.ContainsKey(type))
            {
                Debug.LogError($"이미 등록된 타입: {type}");
                return;
            }
            
            scopeObject.Initialize();
            _registerTrigger.Value = scopeObject;
            _registeredTable.Add(type, scopeObject);
        }

        public void UnRegister<T>() where T : class, IScopeObject
        {
            var type = typeof(T);
            if (_registeredTable.Remove(type, out var value))
            {
                value.Release();
                return;
            }
            
            Debug.LogError($"등록되지 않은 타입: {type}");
        }

        public T Get<T>() where T : class, IScopeObject
        {
            var type = typeof(T);
            if (_registeredTable.TryGetValue(type, out var value))
            {
                return value as T;
            }

            return null;
        }

        public bool TryGet<T>(out T result) where T : class, IScopeObject
        {
            var type = typeof(T);
            if (_registeredTable.TryGetValue(type, out var value))
            {
                result = value as T;
                return true;
            }

            result = null;
            return false;
        }

        public bool Contains<T>() where T : class, IScopeObject
            => _registeredTable.ContainsKey(typeof(T));

        public async UniTask<T> GetAsync<T>(CancellationToken? cancellationToken = default) where T : class, IScopeObject
        {
            if (TryGet<T>(out var result))
            {
                return result;
            }

            var type = typeof(T);

            var token = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken ?? default,
                _cancellation.Token
            ).Token;
            
            while (true)
            {
                IScopeObject changedValue = await _registerTrigger.WaitAsync(token);
                
                if (changedValue.GetType() == type)
                {
                    return changedValue as T;
                }
            }
        }
    }
}