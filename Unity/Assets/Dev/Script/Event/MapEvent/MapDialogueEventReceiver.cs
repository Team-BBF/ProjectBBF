using System;
using System.Collections;
using System.Collections.Generic;
using DS.Core;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;

[Serializable]
[GameData]
public class MapDialogueEventPersistence
{
    public bool IsPlayed;
}
public class MapDialogueEventReceiver : MonoBehaviour
{
    [SerializeField] private string _eventKey;
    [SerializeField] private bool _once = true;

    [SerializeField] private DialogueContainer _container;

    public DialogueEvent DequeueDialogueEvent()
    {
        var data = PersistenceManager.Instance.LoadOrCreate<MapDialogueEventPersistence>(_eventKey);

        if (data.IsPlayed && _once)
        {
            return DialogueEvent.Empty;
        }

        data.IsPlayed = true;

        return new DialogueEvent()
        {
            Container = _container,
            Type = DialogueBranchType.Dialogue
        };
    }

    public DialogueEvent PeekDialogueEvent()
    {
        var data = PersistenceManager.Instance.LoadOrCreate<MapDialogueEventPersistence>(_eventKey);

        if (data.IsPlayed && _once)
        {
            return DialogueEvent.Empty;
        }

        return new DialogueEvent()
        {
            Container = _container,
            Type = DialogueBranchType.Dialogue
        };
    }
}
