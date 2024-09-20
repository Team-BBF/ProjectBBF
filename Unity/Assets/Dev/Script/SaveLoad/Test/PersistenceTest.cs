using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MyBox;
using UnityEngine;

namespace ProjectBBF.Persistence.Test
{
    using ProjectBBF.Persistence;

    [GameData]
    [Serializable]
    public class PersistenceTestObject
    {
        public int Data1;
        public string Data2;
        public float Data3;
        public bool Data4;
    }

    public class PersistenceTest : MonoBehaviour
    {
        [SerializeField] private List<string> _keyList;
        [SerializeField] private List<PersistenceTestObject> _objList;


        [ButtonMethod]
        private void Load()
        {
            PersistenceManager.Instance.LoadGameDataCurrentFileName();

            foreach (var key in _keyList)
            {
                var obj = PersistenceManager.Instance.LoadOrCreate<PersistenceTestObject>(key);

                _objList.Add(obj);
            }
        }

        [ButtonMethod]
        private void Save()
        {
            PersistenceManager.Instance.SaveGameDataCurrentFileName();
        }
    }
}