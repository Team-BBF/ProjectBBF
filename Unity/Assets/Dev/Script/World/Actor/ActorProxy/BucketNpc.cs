using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

public class BucketNpc : Npc
{
    [SerializeField] private BucketFavorability _favorability;
    public BucketFavorability Favorability => _favorability;
    public CollisionInteraction Interaction => Owner.Interaction;

    protected override void OnInit()
    {
        base.OnInit();

        _favorability.Init(Owner);
        ContractInfo.AddBehaivour<IBODialogue>(Favorability);
    }

    protected override void UpdateDefaultInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        if (pc.Interactor.CloserObject != Owner.Interaction) return;
        
        _favorability.UpdateBucket(pc);
        
        var clickObj = pc.Interactor.FindClickObject();
        if (clickObj && clickObj.ContractInfo == Interaction.ContractInfo)
        {
            _ = pc.Dialogue.RunDialogueFromInteraction(Owner.Interaction)
                .ContinueWith(_ =>
                {
                    try
                    {
                        _favorability.ClearBucket();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
        }
    }



    protected override void UpdateHittedInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        _ = pc.Dialogue.RunDialogue(_favorability.HittedDialogue, _favorability.ProcessorData, Owner.transform.position);
    }
}