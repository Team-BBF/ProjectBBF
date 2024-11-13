using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.AddressableAssets;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class FavorabilityDataTable : ScriptableObject
{
    [SerializeField] private List<FavorabilityData> _list;

    public List<FavorabilityData> List => _list;

    public void SetListInEditor(List<FavorabilityData> list)
    {
        _list = list;
    }
}

#if UNITY_EDITOR
public class FavorabilityDataTableBuild
{
    public int callbackOrder => 0;
    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        Load();
    }

    [MenuItem("Build/FavorabilityTable Build")]
    public static void Load()
    {
        var assetGuid = AssetDatabase.FindAssets("t:FavorabilityData", new[] { "Assets/Dev/Data/Actor" });

        List<FavorabilityData> list = new(assetGuid.Length);

        foreach (string guid in assetGuid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<FavorabilityData>(path);

            if (data == null)
            {
                Debug.LogError($"경로({path})에 있는 {typeof(FavorabilityData)}를 불러오기를 실패했습니다.");
                continue;
            }

            list.Add(data);
        }

        var tablePath = "Assets/Dev/Data/Resources/Data/FavorabilityTable.asset";
        FavorabilityDataTable table = AssetDatabase.LoadAssetAtPath<FavorabilityDataTable>(tablePath);

        if (table == null)
        {
            table = ScriptableObject.CreateInstance<FavorabilityDataTable>();
            AssetDatabase.CreateAsset(table, tablePath);
        }

        table.SetListInEditor(list);
        AssetDatabase.SaveAssets(); // 마지막에 한 번만 호출
    }
}
#endif

[Singleton(ESingletonType.Global, -10)]
public class ActorDataManager : MonoBehaviourSingleton<ActorDataManager>
{
    private Dictionary<string, FavorabilityData> _cachedTable;

    public Dictionary<string, FavorabilityData> CachedDict=> _cachedTable;

    
    public override void PostInitialize()
    {
        ResourcesInit();
    }

    private void ResourcesInit()
    {
        #if UNITY_EDITOR
        FavorabilityDataTableBuild.Load();
        #endif
        
        var favorTable = Resources.Load<FavorabilityDataTable>("Data/FavorabilityTable");

        if (favorTable == false)
        {
            Debug.LogError("FavorabilityDataTable 로드 실패 ");
            return;
        }

        _cachedTable = new();
        
        foreach (var favorabilityData in favorTable.List)
        {
            if(favorabilityData == false)continue;
            
            if (_cachedTable.TryGetValue(favorabilityData.ActorKey, out var alreadyData))
            {
                Debug.LogError($"key({favorabilityData.ActorKey}) file({favorabilityData.name})가 이미 존재. 기존 key({alreadyData.ActorKey}) file({alreadyData.name})");
                continue;
            }
            
            _cachedTable.Add(favorabilityData.ActorKey, favorabilityData);
        }
    }
    
    private void AddressableInit()
    {
        var resList = Addressables.LoadResourceLocationsAsync("ActorFavor", typeof(FavorabilityData)).WaitForCompletion();
        _cachedTable = new();

        if (resList is null)
        {
            Debug.LogError("Resources Location을 찾을 수 없음.");
            return;
        }

        var favorList = Addressables.LoadAssetsAsync<FavorabilityData>(resList, null).WaitForCompletion();

        if (favorList is null)
        {
            Debug.LogError("favoraData를 찾을 수 없음.");
            return;
        }
        
        foreach (var favorabilityData in favorList)
        {
            if (_cachedTable.TryGetValue(favorabilityData.ActorKey, out var alreadyData))
            {
                Debug.LogError($"key({favorabilityData.ActorKey}) file({favorabilityData.name})가 이미 존재. 기존 key({alreadyData.ActorKey}) file({alreadyData.name})");
                continue;
            }
            
            _cachedTable.Add(favorabilityData.ActorKey, favorabilityData);
        }
    }

    public override void PostRelease()
    {
        _cachedTable = null;
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
