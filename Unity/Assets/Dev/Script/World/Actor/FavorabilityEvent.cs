using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DS.Core;
using UnityEngine;

[System.Serializable]
public class FavorabilityEventItem
{
    [SerializeField]
    private DialogueContainer _container;
    [SerializeField]
    private int _favorablity;

    [SerializeField] private bool _once;

    public DialogueContainer Container => _container;

    public int Favorablity => _favorablity;

    public bool Once => _once;
}

[System.Serializable]
public class FavorabilityEvent
{

    [SerializeField] private List<FavorabilityEventItem> _eventItems;

    public IReadOnlyList<FavorabilityEventItem> EventItems => _eventItems;
    
}

public class FavorablityContainer
{
    private FavorabilityEvent _event;

    private List<string> _executedDialogueGuid;

    public FavorabilityEvent Event => _event;
    public int CurrentFavorablity { get; set; }

    public IReadOnlyList<string> ExecutedDialogueGuid => _executedDialogueGuid;


    public FavorablityContainer(FavorabilityEvent @event, int currentFavorablity, List<string> executedDialogueGuid)
    {
        _event = @event;
        CurrentFavorablity = currentFavorablity;
        _executedDialogueGuid = executedDialogueGuid ?? new List<string>(2);
    }

    /// <summary>
    /// 실행가능한 다이얼로그들을 반환합니다.
    /// ExecutedDialogueGuid에 포함된 것들을 제외하고 반환합니다. 
    /// </summary>
    /// <returns></returns>
    public List<FavorabilityEventItem> GetExecutableDialogues()
    {
        return Event.EventItems
            .Where(x => _executedDialogueGuid.Contains(x.Container.Guid) == false)
            .ToList();
    }

    public void AddExecutedDialogueGuid(string guid)
    {
        _executedDialogueGuid.Add(guid);
    }

    public bool RemoveExecutedDialogueGuid(string guid)
    {
        return _executedDialogueGuid.Remove(guid);
    }
    
}