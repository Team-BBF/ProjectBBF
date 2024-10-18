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
    public FavorablityContainer FavorablityContainer { get; private set; }
    
    public override DialogueEvent DequeueDialogueEvent()
    {
        // TODO: 테스트 코드
        if (FavorablityContainer.Event.EventItems.Count == 0) return DialogueEvent.Empty;

        FavorabilityEventItem eventItem = FavorablityContainer.Event.EventItems[0];
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
        if (FavorablityContainer.Event.EventItems.Count == 0) return DialogueEvent.Empty;

        return new DialogueEvent()
        {
            Container = FavorablityContainer.Event.EventItems[0].Container,
            Type = DialogueBranchType.Dialogue | DialogueBranchType.Exit,
            ProcessorData = ProcessorData
        };
    }

    public override void Init(Actor actor)
    {
        base.Init(actor);
        
        if (ActorDataManager.Instance.CachedDict.TryGetValue(actor.ActorKey, out var data))
        {
            FavorablityContainer = new FavorablityContainer(data.FavorabilityEvent, 0, null);
        }
        else
        {
            Debug.LogError($"actor key({actor.ActorKey})를 찾을 수 없음");
        }
    }
}