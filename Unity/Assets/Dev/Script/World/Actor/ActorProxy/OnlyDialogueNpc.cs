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


    public override void UpdateInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        if (InputManager.Map.Player.InteractionDialogue.triggered)
        {
            _ = pc.Dialogue.RunDialogueFromInteraction(Interaction);
        }
    }
}