using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;


public abstract class Npc : ActorProxy, IBOInteractiveMulti, IBOInteractiveTool
{
    [SerializeField] private GameObject _wetEmotion;
    [SerializeField] private float _wetEmotionDuration;
    [SerializeField] private ParticlePlayer _hittedEffect;
    [SerializeField] private string _hittedAudioGroup;
    [SerializeField] private string _hittedAudioKey;
    
    protected override void OnInit()
    {
        ContractInfo.AddBehaivour<IBOInteractiveMulti>(this);
        ContractInfo.AddBehaivour<IBOInteractiveTool>(this);

        if (_wetEmotion)
        {
            _wetEmotion.SetActive(false);
        }
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
                WetEmotionAsync().Forget(Debug.LogError);
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

    private async UniTask WetEmotionAsync()
    {
        if (this == false) return;
        if (_wetEmotion == false) return;
        
        _wetEmotion.SetActive(true);

        _ = await UniTask.Delay(TimeSpan.FromSeconds(_wetEmotionDuration), DelayType.DeltaTime, PlayerLoopTiming.Update,
            this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();

        if (_wetEmotion)
        {
            _wetEmotion.SetActive(false);
        }
        
    }

    protected abstract void UpdateDefaultInteract(CollisionInteractionMono caller);
    protected abstract void UpdateHittedInteract(CollisionInteractionMono caller);
    protected abstract void UpdateWetInteract(CollisionInteractionMono caller);
}