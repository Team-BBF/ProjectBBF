using System;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [GameData, Serializable]
    public class CollectPersistenceObject : ISaveLoadNotification
    {
        [SerializeField] private bool _isCollected;
        [SerializeField] private int _collectCount;
        [SerializeField] private bool _saved;

        public bool IsCollected => _isCollected;

        public int CollectCount => _collectCount;
        
        public CollectingMovedActor Actor { get; set; }
        public CollectingObject Object { get; set; }

        public bool Saved
        {
            get => _saved;
            set => _saved = value;
        }

        public void Save(CollectingObject obj)
        {
            _isCollected = obj.IsCollected;
        }
        public void Load(CollectingObject obj)
        {
            obj.IsCollected = _isCollected;
        }
        public void Save(CollectingMovedActor obj)
        {
            _collectCount = obj.CollectCount;
        }
        public void Load(CollectingMovedActor obj)
        {
            obj.CollectCount = _collectCount;
        }

        public void OnSavedNotify()
        {
            if (Actor)
            {
                Save(Actor);
            }

            if (Object)
            {
                Save(Object);
            }
        }

        public void OnLoadedNotify()
        {
            if (Actor)
            {
                Load(Actor);
            }

            if (Object)
            {
                Load(Object);
            }
        }
    }
}