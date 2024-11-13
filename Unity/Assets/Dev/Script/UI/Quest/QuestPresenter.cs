



using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.UI;

public class QuestPresenter : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Transform _content;
    [SerializeField] private QuestView _originPrefab;

    private List<QuestView> _viewList = new(5);

    private void Start()
    {
        QuestManager.Instance.ESO.OnEventRaised += QuestUpdate;
        
        var obj = PersistenceManager.Instance.LoadOrCreate<QuestPersistence>(QuestManager.PERSISTENCE_KEY);

        foreach (KeyValuePair<string,QuestType> data in obj.QuestTable)
        {
            if (data.Value == QuestType.Create)
            {
                QuestUpdate(new QuestEvent()
                {
                    Type = QuestType.Create,
                    QuestKey = data.Key
                });
            }
        }
    }

    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.ESO.OnEventRaised -= QuestUpdate;
        }
    }

    public void QuestUpdate(QuestEvent evt)
    {
        if (QuestManager.Instance == false) return;

        var data = QuestManager.Instance.QuestDataTable.GetValueOrDefault(evt.QuestKey);
        if (data == false)
        {
            Debug.LogError($"존재하지 않는 QuestData, key({evt.QuestKey})");
            return;
        }
        
        switch (evt.Type)
        {
            case QuestType.Create:
                CreateView(data);
                break;
            case QuestType.Complete:
                RemoveView(data.QuestKey);
                break;
            case QuestType.Cancele:
                RemoveView(data.QuestKey);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CreateView(QuestData data)
    {
        Debug.Assert(_originPrefab);
        
        var obj = GameObject.Instantiate(_originPrefab, _content, false);
        obj.gameObject.SetActive(true);
        obj.SetData(data);
        obj.name = data.QuestKey;
        _viewList.Add(obj);
    }

    private void RemoveView(string key)
    {
        int index = _viewList.FindIndex(x => x && x.Data & x.Data.QuestKey == key);
        if (index == -1)
        {
            return;
        }
        
        _viewList[index].DestroySelf();
        _viewList.RemoveAt(index);
    }
}