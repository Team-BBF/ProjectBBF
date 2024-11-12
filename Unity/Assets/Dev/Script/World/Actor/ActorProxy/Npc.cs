using ProjectBBF.Event;
using UnityEngine;


public abstract class Npc : ActorProxy, IBOInteractive
{
    protected override void OnInit()
    {
        ContractInfo.AddBehaivour<IBOInteractive>(this);
    }

    protected override void OnDoDestroy()
    {
    }

    public CollisionInteraction Interaction => Owner.Interaction;
    public abstract void UpdateInteract(CollisionInteractionMono caller);
}