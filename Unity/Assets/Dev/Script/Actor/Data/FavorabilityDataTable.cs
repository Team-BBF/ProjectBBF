using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;


[CreateAssetMenu(menuName = "ProjectBBF/Data/Actor/FavorabilityDataTable", fileName = "FavorabilityDataTable")]
public class FavorabilityDataTable : ScriptableObject
{
    [SerializeField] private List<FavorabilityData> _datas = new();

    private Dictionary<string, FavorabilityData> _table;

    public Dictionary<string, FavorabilityData> Table
    {
        get
        {
            if (_table is null)
            {
                _table = new Dictionary<string, FavorabilityData>();
                
                _datas.ForEach(x=>_table.Add(x.ActorKey, x));
            }

            return _table;
        }
    }


    public Sprite GetPortraitFromKey(string portraitKey)
    {
        foreach (FavorabilityData data in Table.Values)
        {
            if (data.PortraitTable.Table.TryGetValue(portraitKey, out var sprite))
            {
                return sprite;
            }
        }

        return null;
    }
}

[Singleton(ESingletonType.Global, -10)]
public class ActorDataManager : MonoBehaviourSingleton<ActorDataManager>
{
    public FavorabilityDataTable Table { get; private set; }

    public override void PostInitialize()
    {
        Table = Resources.Load<FavorabilityDataTable>("Data/FavorabilityTable");
        Debug.Assert(Table is not null);
    }

    public override void PostRelease()
    {
        Table = null;
    }
}
