using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectBBF.Singleton
{
    public static class SingletonFactory
    {
        private static IMonoBehaviourSingleton CreateMonoBehaviour(Type type)
        {
            var gameObject = new GameObject(type.Name);
            Object.DontDestroyOnLoad(gameObject);
            IMonoBehaviourSingleton singleton = gameObject.AddComponent(type) as IMonoBehaviourSingleton;
            
            Debug.Assert(singleton != null, "failed to create MonoBehaviour singleton");
            return singleton;
        }

        private static IGeneralSingleton CreateGeneralSingleton(Type type)
        {
            var singleton = Activator.CreateInstance(type) as IGeneralSingleton;
            
            Debug.Assert(singleton != null, "failed to create General singleton");
            return singleton;
        }

        internal static ISingleton CreateSingleton(Type type)
        {
            ISingleton singleton = null;
            
            if (typeof(IMonoBehaviourSingleton).IsAssignableFrom(type))
            {
                singleton = CreateMonoBehaviour(type);
            }
            else if (typeof(IGeneralSingleton).IsAssignableFrom(type))
            {
                singleton = CreateGeneralSingleton(type);
            }
            else
            {
                Debug.Assert(singleton != null, $"failed to create singleton({type})");
            }
            

            return singleton;
        }
        
    }
}