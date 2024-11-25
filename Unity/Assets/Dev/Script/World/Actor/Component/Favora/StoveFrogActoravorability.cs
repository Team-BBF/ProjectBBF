using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DS.Core;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;

public class StoveFrogActoravorability: ActorFavorablity
{
    [SerializeField] private DialogueContainer _stoveFrogDialogue;
    
    public FavorabilityEvent FavorablityEvent => FavorabilityData.FavorabilityEvent;
    
    public override DialogueEvent DequeueDialogueEvent()
    {
        // TODO: 테스트 코드
        if (FavorablityEvent.EventItems.Count == 0) return DialogueEvent.Empty;

        
#if STOVE_BUILD
        return new DialogueEvent()
        {
            Container = _stoveFrogDialogue,
            Type = DialogueBranchType.Dialogue,
            ProcessorData = ProcessorData
        };
#else
        FavorabilityEventItem eventItem = FavorablityEvent.EventItems[0];
        return new DialogueEvent()
        {
            Container = eventItem.Container,
            Type = eventItem.BranchType,
            ProcessorData = ProcessorData
        };
#endif
    }

    public override DialogueEvent PeekDialogueEvent()
    {
        // TODO: 테스트 코드
        if (FavorablityEvent.EventItems.Count == 0) return DialogueEvent.Empty;

        
#if STOVE_BUILD
        return new DialogueEvent()
        {
            Container = _stoveFrogDialogue,
            Type = DialogueBranchType.Dialogue,
            ProcessorData = ProcessorData
        };
#else
        FavorabilityEventItem eventItem = FavorablityEvent.EventItems[0];
        return new DialogueEvent()
        {
            Container = eventItem.Container,
            Type = DialogueBranchType.Dialogue | DialogueBranchType.Exit,
            ProcessorData = ProcessorData
        };
#endif
    }
}