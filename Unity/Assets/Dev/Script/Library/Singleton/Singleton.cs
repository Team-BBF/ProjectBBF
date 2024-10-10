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
    public enum ESingletonType
    {
        Global,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {
        internal int InitializeOrder;
        internal ESingletonType Type;

        public SingletonAttribute(ESingletonType type, int initializeOrder = 0)
        {
            this.Type = type;
            this.InitializeOrder = initializeOrder;
        }
    }

    public class Singleton : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSplashScreen()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(type =>
                    {
                        var att = type.GetCustomAttribute(typeof(SingletonAttribute), true);

                        if (att == null) return false;
                        if (att is not SingletonAttribute { Type: ESingletonType.Global }) return false;

                        return true;
                    })
                    .Where(type => type.IsClass)
                    .ToList();

                // 내림차순 정렬
                types.Sort((x, y) =>
                {
                    var att1 = x.GetCustomAttribute(typeof(SingletonAttribute), true);
                    var att2 = y.GetCustomAttribute(typeof(SingletonAttribute), true);

                    if (att1 is SingletonAttribute sAtt1 && att2 is SingletonAttribute sAtt2)
                    {
                        if (sAtt1.InitializeOrder < sAtt2.InitializeOrder) return -1;
                        if (sAtt1.InitializeOrder > sAtt2.InitializeOrder) return 1;
                        else return 0;
                    }

                    return 0;
                });

                foreach (var type in types)
                {
                    var singleton = SingletonFactory.CreateSingleton(type);

                    singleton.Initialize();
                    _singletonList.Add(singleton);
                }
            }


            var obj = new GameObject("__Singleton__").AddComponent<Singleton>();
            DontDestroyOnLoad(obj);
        }

        private void OnApplicationQuit()
        {
            foreach (var singleton in _singletonList)
            {
                singleton.Release();
            }

            _singletonList.Clear();
        }

        private static List<ISingleton> _singletonList = new(5);

        public static T GetSingleton<T>() where T : class, ISingleton
        {
            foreach (ISingleton obj in _singletonList)
            {
                if (obj is T singleton)
                {
                    return singleton;
                }
            }

            throw new Exception($"failed to get ({typeof(T).Name}) singleton");
        }
    }
}