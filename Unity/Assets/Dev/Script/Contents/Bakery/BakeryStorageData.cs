


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Bakery/Storage", fileName = "New Storage")]
public class BakeryStorageData : ScriptableObject
{
    [SerializeField] private bool _infinity;
    [SerializeField] private string _persistenceKey;


    [SerializeField] private List<ItemDataSerializedSet> _defaultItemList;

    public bool Infinity => _infinity;

    public string PersistenceKey => _persistenceKey;
    public List<ItemDataSerializedSet> DefaultItemList => _defaultItemList;
}