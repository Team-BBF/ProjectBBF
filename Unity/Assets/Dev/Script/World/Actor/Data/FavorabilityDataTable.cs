using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;


[CreateAssetMenu(menuName = "ProjectBBF/Project/FavorabilityDataTable", fileName = "FavorabilityDataTable")]
public class FavorabilityDataTable : ScriptableObject
{
    [SerializeField] private List<FavorabilityData> _datas = new();

    public Dictionary<string, FavorabilityData> CreateCachedTable()
    {
        var table = new Dictionary<string, FavorabilityData>();
                
        _datas.ForEach(x=>table.Add(x.ActorKey, x));

        return table;
    }
}

[Singleton(ESingletonType.Global, -10)]
public class ActorDataManager : MonoBehaviourSingleton<ActorDataManager>
{
    public FavorabilityDataTable Table { get; private set; }
    private Dictionary<string, FavorabilityData> _cachedTable;

    public Dictionary<string, FavorabilityData> CachedDict=> _cachedTable;

    public override void PostInitialize()
    {
        Table = Resources.Load<FavorabilityDataTable>("Data/FavorabilityTable");
        Debug.Assert(Table is not null);
        
        _cachedTable = Table.CreateCachedTable();
    }

    public override void PostRelease()
    {
        Table = null;
    }
    
    public Sprite GetPortraitFromKey(string portraitKey)
    {
        if (string.IsNullOrEmpty(portraitKey)) return null;
        
        foreach (FavorabilityData data in _cachedTable.Values)
        {
            if (data.PortraitTable.Table.TryGetValue(portraitKey, out var sprite))
            {
                return sprite;
            }
        }

        return null;
    }
}
