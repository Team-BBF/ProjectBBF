﻿


using System;
using System.Collections.Generic;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Animal : ActorProxy, IBOInteractive
{
    [SerializeField] private ActorFavorablity _favorablity;
    [SerializeField] private CollectingMovedActor _collectingMovedActor;
    [SerializeField] private AnimalAnimator _animalAnimator;

    [SerializeField] private List<RuntimeAnimatorController> _collectedAnimations; 
    [SerializeField] private List<RuntimeAnimatorController> _defaultAnimations; 


    public CollisionInteraction Interaction => Owner.Interaction;
    private IBOInteractive _collectBehaviour;
    public void UpdateInteract(CollisionInteractionMono caller)
    {
        if (_collectBehaviour is null) return;
        
        _collectBehaviour.UpdateInteract(caller);
        _favorablity.UpdateInteract(caller);
    }
    protected override void OnInit()
    {
        _favorablity.Init(Owner);
        _collectingMovedActor.Init(Owner);
        _animalAnimator.Init(Owner);

        var collect = _collectingMovedActor.CreateCollectProxyOrNull();

        ContractInfo.AddBehaivour<IBODialogue>(_favorablity);
        
        // IBOInteractiveTool이면 대화 안 되도록 처리
        if (collect is IBOInteractiveTool)
        {
            ContractInfo.AddBehaivour<IBOInteractiveTool>(collect);
        }
        else if (collect is IBOInteractive)
        {
            _collectBehaviour = collect as IBOInteractive;
            ContractInfo.AddBehaivour<IBOInteractive>(this);
        }
        
        
        _collectingMovedActor.OnChangedCollectingState.AddListener(OnChangedCollectingState);
    }

    protected override void OnDoDestroy()
    {
        _collectingMovedActor.OnChangedCollectingState.RemoveListener(OnChangedCollectingState);
    }


    private void OnChangedCollectingState(CollectState state)
    {
        if (state == CollectState.Collected)
        {
            SetRandomAnimator(_collectedAnimations);
        }
        else
        {
            SetRandomAnimator(_defaultAnimations);
        }
    }

    private void SetRandomAnimator(List<RuntimeAnimatorController> anis)
    {
        if (anis.Count == 0) return;
        int index = Random.Range(0, anis.Count - 1);

        Owner.Visual.RuntimeAnimator = anis[index];
    }
}