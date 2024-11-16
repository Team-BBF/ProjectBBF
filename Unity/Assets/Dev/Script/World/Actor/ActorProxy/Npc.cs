using ProjectBBF.Event;
using UnityEngine;


public abstract class Npc : ActorProxy, IBOInteractiveMulti
{
    protected override void OnInit()
    {
        ContractInfo.AddBehaivour<IBOInteractiveMulti>(this);
    }

    protected override void OnDoDestroy()
    {
    }

    public CollisionInteraction Interaction => Owner.Interaction;
    public abstract void UpdateInteract(CollisionInteractionMono caller);
}