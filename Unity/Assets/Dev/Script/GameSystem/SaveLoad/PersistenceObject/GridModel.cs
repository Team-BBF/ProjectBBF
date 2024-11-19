using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [Serializable]
    public struct GridModelSerialized
    {
        public List<Vector2Int> Pos;
        public List<string> ItemKey;
        public List<int> Count;

        public bool Empty
        {
            get
            {
                if (Pos is null || ItemKey is null || Count is null) return true;
                if(Pos.Count == 0 || ItemKey.Count == 0 || Count.Count == 0) return false;

                return true;
            }
        }
    }

    
    [GameData, Serializable]
    public class GridModelPersistenceObject : ISaveLoadNotification
    {
        [SerializeField] private bool _saved;
        [SerializeField] private GridModelSerialized _serialized;
        public GridInventoryModel Model { get; set; }

        public bool Saved
        {
            get => _saved;
            set => _saved = value;
        }

        public void SaveModle(GridInventoryModel model)
        {
            _serialized = new GridModelSerialized
            {
                ItemKey = new List<string>(model.MaxSize),
                Count = new List<int>(model.MaxSize),
                Pos = new List<Vector2Int>(model.MaxSize)
            };

            using var e = model.GetEnumerator();
            while (e.MoveNext())
            {
                if(e.Current is not GridInventorySlot slot) return;
                if (slot?.Empty ?? true) continue;
                if (slot.Data == false) continue;
            
                _serialized.ItemKey.Add(slot.Data.ItemKey);
                _serialized.Count.Add(slot.Count);
                _serialized.Pos.Add(slot.Position);
            }
        }
        public void LoadModel(GridInventoryModel model)
        {
            Debug.Assert(model is not null);
            
            if (_serialized.Pos is null) return;
            if (_serialized.ItemKey is null) return;
            if (_serialized.Count is null) return;
        
            int length = Mathf.Min(
                _serialized.Pos.Count,
                _serialized.ItemKey.Count,
                _serialized.Count.Count
            );
        
            Debug.Assert(_serialized.Pos.Count == length);
            Debug.Assert(_serialized.ItemKey.Count == length);
            Debug.Assert(_serialized.Count.Count == length);

            IItemDataResolver resolver = null;
            if (DataManager.Instance)
            {
                resolver = DataManager.Instance.GetResolver<IItemDataResolver>();
            }
            
            if (resolver is null)
            {
                Debug.LogError($"아이템을 불러올 수 없습니다. key({model.PersistenceKey})");
                return;
            }


            for (int i = 0; i < length; i++)
            {
                Vector2Int pos = _serialized.Pos[i];
                if (model.Size.x <= pos.x || model.Size.y <= pos.y || pos.y < 0 || pos.x < 0)
                {
                    Debug.LogWarning($"올바르지 않은 pos, key({model.PersistenceKey})");
                    continue;
                }

                if (resolver.TryGetData(_serialized.ItemKey[i], out ItemData itemData) is false)
                {
                    Debug.LogError($"아이템을 불러올 수 없습니다. persistence key({model.PersistenceKey}), item key({_serialized.ItemKey[i]})");
                    continue;
                }
                
                model.Slots[pos.y, pos.x].ForceSet(itemData, _serialized.Count[i]);
            }
            
        }
        
        public void OnSavedNotify()
        {
            if (Model is null) return;
            
            SaveModle(Model);
        }

        public void OnLoadedNotify()
        {
            if (Model is null) return;
            
            LoadModel(Model);
            Model.ApplyChanged();
        }
    }
}