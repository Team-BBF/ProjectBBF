


using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Persistence;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[Serializable]
public enum QuestType
{
    NotExist,
    Create,
    Complete,
    Cancele,
}

[Singleton(ESingletonType.Global, 10)]
public class QuestManager : MonoBehaviourSingleton<QuestManager>
{
    private const string TABLE_PATH = "Data/Quest";

    public Dictionary<string, QuestData> _questDataTable;
    public IReadOnlyDictionary<string, QuestData> QuestDataTable => _questDataTable;

    public const string PERSISTENCE_KEY = "Quest";
    public const string ESO_PATH = "Event/ESO_Quest";
    
    public ESOQuest ESO { get; private set; }
    
    public List<QuestIndicatorObstacleUI> IndicatorObstacleList { get; private set; }
    public List<QuestIndicatorUI> IndicatorList { get; private set; }

    private QuestPersistence _persistence;

    public override void PostInitialize()
    {
        ESO = Resources.Load<ESOQuest>(ESO_PATH);
        QuestData[] arr = Resources.LoadAll<QuestData>(TABLE_PATH);

        _questDataTable = new Dictionary<string, QuestData>(arr.Select(x => new KeyValuePair<string, QuestData>(x.QuestKey, x)));
        
        if (PersistenceManager.Instance)
        {
            PersistenceManager.Instance.OnGameDataLoaded += OnLoaded;
            _persistence = PersistenceManager.Instance.LoadOrCreate<QuestPersistence>(QuestManager.PERSISTENCE_KEY);
        }
        
        Debug.Assert(ESO);

        IndicatorObstacleList = new(5);
        IndicatorList = new(5);
    }

    private void OnLoaded(PersistenceManager obj)
    {
        _persistence = obj.LoadOrCreate<QuestPersistence>(QuestManager.PERSISTENCE_KEY);
    }

    public override void PostRelease()
    {
        _questDataTable?.Clear();
        _questDataTable = null;
        
        IndicatorObstacleList?.Clear();
        IndicatorObstacleList = null;
    }

    public QuestType GetQuestState(string key)
    {
        if (_persistence.QuestTable.TryGetValue(key, out var curType))
        {
            return curType;
        }

        return QuestType.NotExist;
    }

    public QuestType GetQuestState(QuestData questData) => GetQuestState(questData.QuestKey);
    public bool ChangeQuestState(string key, QuestType type)
    {
        if (QuestDataTable.TryGetValue(key, out QuestData value))
        {
            return ChangeQuestState(value, type);
        }
        else
        {
            Debug.LogError($"Quest를 찾을 수 없습니다. key({key})");
        }

        return false;
    }

    public bool ChangeQuestState(QuestData questData, QuestType type)
    {
        if (_persistence.QuestTable.TryGetValue(questData.QuestKey, out var curType))
        {
            if (curType == type)
            {
                return false;
            }
        }
        
        _persistence.QuestTable[questData.QuestKey] = type;
        return true;
    }

    public IList<(string questKey, QuestType state)> GetAllQuestState()
    {
        return _persistence.QuestTable.Select(x => (x.Key, x.Value)).ToList();
    }
}