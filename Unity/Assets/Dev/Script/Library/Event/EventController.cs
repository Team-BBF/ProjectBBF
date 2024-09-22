using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ProjectBBF.Singleton;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Type = System.Type;


namespace ProjectBBF.Event
{
    public interface IEventCommand
    {
    }

    public abstract class EventBridge
    {
        public abstract IEventCommand GetEventCommand();
    }

    public class EventPageAttribute : Attribute
    {
    }

    [Singleton(ESingletonType.Global)]
    public class EventController : MonoBehaviourSingleton<EventController>
    {
        private delegate void EventHandler(IEventCommand command);

        private Dictionary<Type, List<EventHandler>> _eventPageTable;
        private Dictionary<Type, EventBridge> _bridgeTable;

        public override void PostInitialize()
        {
            _eventPageTable = new();
            _bridgeTable = new();


            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<(Type, MethodInfo)> allEventPageMethod= new List<(Type, MethodInfo)>(10);

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(type =>
                    {
                        var att = type.GetCustomAttribute(typeof(EventPageAttribute), false);
                        if (att is null) return false;

                        return true;
                    })
                    .Where(type => type.IsClass)
                    .ToList();

                types.ForEach(x =>
                {
                    var methods = x.GetMethods(
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                    foreach (MethodInfo method in methods)
                    {
                        allEventPageMethod.Add((x, method));
                    }
                });
            }


            foreach (var tuple in allEventPageMethod)
            {
                MethodInfo method = tuple.Item2;
                Type eventPageType = tuple.Item1;
                
                var att = method.GetCustomAttribute<EventHandlerAttribute>();
                if(att is null)continue;

                if (CheckMethodParameter(method, eventPageType) is false) continue;
                
                var action = method.CreateDelegate(typeof(EventHandler)) as EventHandler;
                if (action is null) continue;

                if (_eventPageTable.TryGetValue(att.Type, out var list))
                {
                    list.Add(action);
                }
                else
                {
                    list = new List<EventHandler>(10);
                    _eventPageTable.Add(att.Type, list);
                    list.Add(action);
                }
            }
        }

        private bool CheckMethodParameter(MethodInfo method, Type eventPageType)
        {
            ParameterInfo parameter = method.GetParameters().FirstOrDefault();
            if (parameter is null || parameter.ParameterType != typeof(IEventCommand))
            {
                Debug.LogError($"EventPage({eventPageType})의 {method.Name}메소드의 매개변수와 호환되지 않습니다.");
                return false;
            }
            return true;
        }

        public override void PostRelease()
        {
            _eventPageTable.Clear();
            _eventPageTable = null;

            _bridgeTable.Clear();
            _bridgeTable = null;
        }

        public T GetBridge<T>() where T : EventBridge, new()
        {
            if (_bridgeTable.TryGetValue(typeof(T), out var bridge))
            {
                return bridge as T;
            }
            else
            {
                bridge = new T();
                _bridgeTable.Add(typeof(T), bridge);
                return bridge as T;
            }
        }

        private void LateUpdate()
        {
            foreach (var bridge in _bridgeTable.Values)
            {
                var command = bridge.GetEventCommand();

                SignalEvent(command);
            }
        }

        public void SignalEvent(IEventCommand command)
        {
            if (_eventPageTable.TryGetValue(command.GetType(), out var list))
            {
                foreach (var handler in list)
                {
                    handler.Invoke(command);
                }
            }
        }
    }

    [System.AttributeUsage(AttributeTargets.Method)]
    public class EventHandlerAttribute : Attribute
    {
        public System.Type Type;

        public EventHandlerAttribute(System.Type type)
        {
            this.Type = type;
        }
    }

    public static partial class EventPage
    {
    }
}