using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace  DS.Runtime
{
    
    public abstract class ParameterHandler : ScriptableObject
    {
        public abstract Type[] GetArgumentTypes();

        protected ProcessorData ProcessorData { get; private set; }

        public object Execute(object[] args, ProcessorData processorData)
        {
            var types = GetArgumentTypes();
            for (int i = 0; i < Mathf.Min(args.Length, types.Length); i++)
            {
                var curType = args[i].GetType();
                if (curType != types[i] &&
                    types[i].IsAssignableFrom(curType) is false
                    )
                {
                    Debug.LogError($"올바르지 않은 타입, input({curType}), defined({types[i]})");
                    return null;
                }
            }

            // 제너릭 메서드를 찾는 로직
            var method = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "OnExecute" &&
                                     m.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
            if (method != null)
            {
                ProcessorData = processorData;
                object result = method.Invoke(this, args);
                ProcessorData = null;
                
                return result;
            }
            else
            {
                Debug.LogError("OnExecute method not found");
            }

            return null;
        }
    }

    public abstract class ParameterHandlerArgsT : ParameterHandler
    {
        public sealed override Type[] GetArgumentTypes()
            => new Type[]{};
    }
    public abstract class ParameterHandlerArgsT<T0> : ParameterHandler
    {
        public sealed override Type[] GetArgumentTypes()
            => new []{ typeof(T0), };
        
        protected abstract object OnExecute(T0 arg0);
    }
    public abstract class ParameterHandlerArgsT<T0, T1> : ParameterHandler
    {
        public sealed override Type[] GetArgumentTypes()
            => new []{ typeof(T0), typeof(T1), };
        
        protected abstract object OnExecute(T0 arg0, T1 arg1);
    }
    public abstract class ParameterHandlerArgsT<T0, T1, T2> : ParameterHandler
    {
        public sealed override Type[] GetArgumentTypes()
            => new []{ typeof(T0), typeof(T1), typeof(T2)};
        
        protected abstract object OnExecute(T0 arg0, T1 arg1, T2 arg2);
    }
    public abstract class ParameterHandlerArgsT<T0, T1, T2, T3> : ParameterHandler
    {
        public sealed override Type[] GetArgumentTypes()
            => new []{ typeof(T0), typeof(T1), typeof(T2), typeof(T3)};
        
        protected abstract object OnExecute(T0 arg0, T1 arg1, T2 arg2, T3 arg3);
    }
    public abstract class ParameterHandlerArgsT<T0, T1, T2, T3, T4> : ParameterHandler
    {
        public sealed override Type[] GetArgumentTypes()
            => new []{ typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4)};
        
        protected abstract object OnExecute(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }
}