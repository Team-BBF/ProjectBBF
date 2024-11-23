using ProjectBBF.Event;
using UnityEngine;


public abstract class Npc : ActorProxy, IBOInteractiveMulti, IBOInteractiveTool
{
    [SerializeField] private ParticlePlayer _hittedEffect;
    [SerializeField] private string _hittedAudioGroup;
    [SerializeField] private string _hittedAudioKey;
    
    protected override void OnInit()
    {
        ContractInfo.AddBehaivour<IBOInteractiveMulti>(this);
        ContractInfo.AddBehaivour<IBOInteractiveTool>(this);
    }

    protected override void OnDoDestroy()
    {
    }

    public CollisionInteraction Interaction => Owner.Interaction;
    public bool IsVaildTool(ToolRequireSet toolSet)
    {
        return toolSet.RequireToolType is 
            ToolType.Hoe or 
            ToolType.Sickle or 
            ToolType.Hammer or 
            ToolType.Pickaxe or
            ToolType.WaterSpray
            ;
    }

    void IBOInteractiveMulti.UpdateInteract(CollisionInteractionMono caller)
    {
        UpdateDefaultInteract(caller);
    }
    void IBOInteractiveTool.UpdateInteract(CollisionInteractionMono caller)
    {


        if (caller.Owner is not PlayerController pc) return;

        if (pc.Inventory.CurrentItemData)
        {
            if (pc.Inventory.CurrentItemData.Info.Contains(ToolType.WaterSpray))
            {
                UpdateWetInteract(caller);
            }
            else
            {
                if (_hittedEffect)
                {
                    _hittedEffect.Play();
                }
                AudioManager.Instance.PlayOneShot(_hittedAudioGroup, _hittedAudioKey);
                UpdateHittedInteract(caller);
            }
        }
    }

    protected abstract void UpdateDefaultInteract(CollisionInteractionMono caller);
    protected abstract void UpdateHittedInteract(CollisionInteractionMono caller);
    protected abstract void UpdateWetInteract(CollisionInteractionMono caller);
}