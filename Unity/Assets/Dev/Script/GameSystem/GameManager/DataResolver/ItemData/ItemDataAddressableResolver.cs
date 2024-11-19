using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ItemDataAddressableResolver : IItemDataResolver
{
    private Dictionary<string, ItemData> _table;
    private bool _isInit;
    
    public bool TryGetData(string key, out ItemData value)
    {
        value = _table.GetValueOrDefault(key);

        return value;
    }

    public bool IsInit() => _isInit;

    public void Init()
    {
        AsyncOperationHandle<IList<IResourceLocation>> locations = Addressables.LoadResourceLocationsAsync(
            "ItemData",
            null
            );
        locations.WaitForCompletion();
        
        
        AsyncOperationHandle<IList<ItemData>> itemHandle = Addressables.LoadAssetsAsync<ItemData>(locations.Result, null);
        itemHandle.WaitForCompletion();

        IEnumerable<KeyValuePair<string, ItemData>> keyPairEnumerator = itemHandle.Result.Select(x => new KeyValuePair<string, ItemData>(x.ItemKey, x));
        _table = new Dictionary<string, ItemData>(keyPairEnumerator);

        _isInit = true;
    }

    public void Release()
    {
        _isInit = false;
        _table.Clear();
    }
}