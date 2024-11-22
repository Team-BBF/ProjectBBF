using System;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [GameData, Serializable]
    public class AhirBoxPersistenceObject
    {
        [SerializeField] private string _itemKey;
        [SerializeField] private string _passItemKey;

        public string PassItemKey
        {
            get => _passItemKey;
            set => _passItemKey = value;
        }

        public string ItemKey
        {
            get => _itemKey;
            set => _itemKey = value;
        }
    }
}