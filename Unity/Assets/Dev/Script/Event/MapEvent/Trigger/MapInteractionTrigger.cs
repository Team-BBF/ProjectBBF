using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;

[RequireComponent(typeof(MapDialogueEventReceiver), typeof(CollisionInteraction))]
public class MapInteractionTrigger : MonoBehaviour, IBADialogue
{
    [field: SerializeField, AutoProperty]
    private MapDialogueEventReceiver _receiver;

    [field: SerializeField, AutoProperty] 
    private CollisionInteraction _interaction;

    public CollisionInteraction Interaction => _interaction;
    
    public DialogueEvent DequeueDialogueEvent()
    {
        return _receiver.DequeueDialogueEvent();
    }

    public DialogueEvent PeekDialogueEvent()
    {
        return _receiver.PeekDialogueEvent();
    }

    private void Awake()
    {
        var info = ActorContractInfo.Create(() => this);
        Interaction.SetContractInfo(info, this);

        info.AddBehaivour<IBADialogue>(this);
    }
}
