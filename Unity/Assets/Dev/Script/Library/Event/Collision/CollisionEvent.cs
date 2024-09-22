using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectBBF.Singleton;

namespace ProjectBBF.Event
{
    public class CollisionEventCommand : IEventCommand, IEqualityComparer<List<CollisionInteraction>>
    {
        public List<List<CollisionInteraction>> groups = new();

        public bool Equals(List<CollisionInteraction> x, List<CollisionInteraction> y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            if (x.Count != y.Count) return false;

            int count = 0;
            for (int i = 0; i < x.Count; i++)
            {
                for (int j = 0; j < y.Count; j++)
                {
                    if (x[i] == y[j])
                    {
                        ++count;
                    }
                }
            }

            return count == x.Count;
        }

        public int GetHashCode(List<CollisionInteraction> obj)
        {
            return HashCode.Combine(obj.Capacity, obj.Count);
        }
    }
    
    public class CollisionBridge : EventBridge
    {
        private Dictionary<CollisionInteraction, List<CollisionInteraction>> _allTable = new();
        
        public override IEventCommand GetEventCommand()
        {
            var command = new CollisionEventCommand();
            
            foreach (var pair in _allTable)
            {
                command.groups.Add(MakeGroup(pair.Key, pair.Value));
            }

            _allTable.Clear();

            command.groups = command.groups.Distinct(command).ToList();
            return command;
        }

        public void Push(CollisionInteraction key, CollisionInteraction value)
        {
            if (_allTable.TryGetValue(key, out var list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<CollisionInteraction>();
                list.Add(value);
                _allTable.Add(key, list);
            }
        }

        public List<CollisionInteraction> MakeGroup(CollisionInteraction key, List<CollisionInteraction> list)
        {
            var group = new List<CollisionInteraction>(list.Count);
            group.Add(key);

            foreach (var value in list)
            {
                group.Add(value);
            }

            return group.Distinct().ToList();
        }
    }
}