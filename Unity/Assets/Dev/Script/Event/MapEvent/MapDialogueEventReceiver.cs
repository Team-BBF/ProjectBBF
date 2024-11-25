using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
public class MapDialogueEventReceiver : MonoBehaviour, IBODialogue
{
    [SerializeField] private MapTriggerBase _trigger;
    [SerializeField] private string _eventKey;
    [SerializeField] private bool _once = true;

    [SerializeField] private DialogueContainer _container;

    public CollisionInteraction Interaction => _trigger.Interaction;

    public virtual DialogueContainer Container => _container;

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
            Container = Container,
            Type = DialogueBranchType.Dialogue
        };
    }

    public DialogueEvent PeekDialogueEvent()
    {
        return new DialogueEvent()
        {
            Container = Container,
            Type = DialogueBranchType.Dialogue
        };
    }

    private void Play(CollisionInteractionMono caller)
    {
        if (caller.Owner is PlayerController pc)
        {
            _ = pc.Dialogue.RunDialogueFromInteraction(Interaction);
        }
    }

    private void Start()
    {
        if (_trigger)
        {
            _trigger.OnTrigger += Play; 
        }
        
        if(Interaction.ContractInfo is ObjectContractInfo info)
        {
            info.AddBehaivour<IBODialogue>(this);
        }

    }

    private void OnDestroy()
    {
        if (_trigger)
        {
            _trigger.OnTrigger -= Play; 
        }
    }
}
