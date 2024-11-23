using ProjectBBF.Event;
using UnityEngine;

public class OnlyDialogueNpc : Npc
{
    [SerializeField] private ActorFavorablity _favorablity;
    
    protected override void OnInit()
    {
        base.OnInit();
        
        _favorablity.Init(Owner);
        ContractInfo.AddBehaivour<IBODialogue>(_favorablity);
    }
    protected override void UpdateDefaultInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        
        var clickObj = pc.Interactor.FindClickObject();
        if (clickObj && clickObj.ContractInfo == Interaction.ContractInfo)
        {
            _ = pc.Dialogue.RunDialogueFromInteraction(Interaction);
        }
    }

    protected override void UpdateHittedInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        _ = pc.Dialogue.RunDialogue(_favorablity.HittedDialogue, _favorablity.ProcessorData, Owner.transform.position);
    }

    protected override void UpdateWetInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        _ = pc.Dialogue.RunDialogue(_favorablity.WetDialogue, _favorablity.ProcessorData, Owner.transform.position);
    }
}