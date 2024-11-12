using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DS.Core;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;

public class ActorFavorablity: ActorComFavorability
{
    public FavorabilityEvent FavorablityEvent => FavorabilityData.FavorabilityEvent;
    
    public override DialogueEvent DequeueDialogueEvent()
    {
        // TODO: 테스트 코드
        if (FavorablityEvent.EventItems.Count == 0) return DialogueEvent.Empty;

        FavorabilityEventItem eventItem = FavorablityEvent.EventItems[0];
        return new DialogueEvent()
        {
            Container = eventItem.Container,
            Type = eventItem.BranchType,
            ProcessorData = ProcessorData
        };
    }

    public override DialogueEvent PeekDialogueEvent()
    {
        // TODO: 테스트 코드
        if (FavorablityEvent.EventItems.Count == 0) return DialogueEvent.Empty;

        return new DialogueEvent()
        {
            Container = FavorablityEvent.EventItems[0].Container,
            Type = DialogueBranchType.Dialogue | DialogueBranchType.Exit,
            ProcessorData = ProcessorData
        };
    }
}