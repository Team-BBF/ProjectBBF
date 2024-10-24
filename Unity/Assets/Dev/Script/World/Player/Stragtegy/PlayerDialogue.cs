using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine.Utility;
using Cysharp.Threading.Tasks;
using DS.Core;
using DS.Runtime;
using JetBrains.Annotations;
using MyBox;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

public class PlayerDialogue : MonoBehaviour, IPlayerStrategy
{
    private PlayerController _controller;
    private PlayerBlackboard _blackboard;

    public void Init(PlayerController controller)
    {
        _controller = controller;
        _blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
    }

    public bool CanDialogueAction
    {
        get
        {
            if (_blackboard.IsInteractionStopped) return false;
            
            var interaction = FindCloserObject();
            if (interaction is null) return false;


            if (interaction.TryGetContractInfo(out ActorContractInfo actorInfo) &&
                actorInfo.TryGetBehaviour(out IBADialogue dialogue))
            {
                if (dialogue.PeekDialogueEvent().IsEmpty)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    public async UniTask<bool> OnDialogueAction()
    {
        
        if (_blackboard.IsInteractionStopped) return false;
        
        try
        {
            var interaction = FindCloserObject();
            if (interaction is null) return false;
            
            bool success = await RunDialogueFromInteraction(interaction);

            if (success)
            {
                return true;
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }

        return false;
    }
    
    public async UniTask<bool> RunDialogueFromInteraction(CollisionInteractionMono caller)
    {
        
        if (_blackboard.IsInteractionStopped) return false;

        try
        {
            if (caller is null) return false;


            if (caller.TryGetContractInfo(out ActorContractInfo actorInfo) &&
                actorInfo.TryGetBehaviour(out IBADialogue dialogue) &&
                dialogue.PeekDialogueEvent().IsEmpty)
            {
                return false;
            }


            _controller.MoveStrategy.ResetVelocity();

            bool success = await BranchDialogue(caller);

            DialogueController.Instance.ResetDialogue();

            if (success)
            {
                return true;
            }
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
        finally
        {
            _controller.Inventory.QuickInvVisible = true;
            _controller.HudController.Visible = true;
            _controller.Blackboard.IsMoveStopped = false;
            _controller.Blackboard.IsInteractionStopped = false;
        }

        return false;
    }

    public async UniTask<bool> RunDialogue(DialogueContainer container, ProcessorData processorData, Vector3? targetPosition = null)
    {
        try
        {
            _controller.Inventory.QuickInvVisible = false;
            _controller.HudController.Visible = false;
            _controller.Blackboard.IsMoveStopped = true;
            _controller.Blackboard.IsInteractionStopped = true;
            _controller.MoveStrategy.ResetVelocity();

            if (targetPosition is not null)
            {
                Vector2 dir = (targetPosition.Value - _controller.transform.position).normalized;
                _controller.VisualStrategy.LookAt(dir, AnimationActorKey.Action.Idle, true);
            }

            var token = this.GetCancellationTokenOnDestroy();

            var instance = DialogueController.Instance;
            instance.ResetDialogue();
            instance.Visible = true;
        
            DialogueContext context = instance.CreateContext(container, processorData);

            await context.Next();

            while (context.CanNext)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                await context.Next();
            }

            instance.Visible = false;

            return true;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
        finally
        {
            _controller.Inventory.QuickInvVisible = true;
            _controller.HudController.Visible = true;
            _controller.Blackboard.IsMoveStopped = false;
            _controller.Blackboard.IsInteractionStopped = false;
        }

        return false;
    }

    private async UniTask<bool> BranchDialogue(CollisionInteractionMono interaction)
    {
        if (interaction.TryGetContractInfo(out ActorContractInfo actorInfo) &&
            actorInfo.TryGetBehaviour(out IBADialogue dialogue))
        {
            var stateTransfer = actorInfo.GetBehaviourOrNull<IBAStateTransfer>();
            var nameKey = actorInfo.GetBehaviourOrNull<IBANameKey>();

            var dialogueEvent = dialogue.DequeueDialogueEvent();

            if (dialogueEvent.IsEmpty)
            {
                stateTransfer?.TranslateState("DailyRoutine");
                return false;
            }

            stateTransfer?.TranslateState("TalkingForPlayer");
            _controller.Inventory.HideAll();
            _controller.HudController.Visible = false;
            _controller.Blackboard.IsMoveStopped = true;
            _controller.Blackboard.IsInteractionStopped = true;
            

            if (actorInfo.Interaction.Owner is Actor actor)
            {
                Vector2 dir = (actor.transform.position - _controller.transform.position).normalized;
                _controller.VisualStrategy.LookAt(dir, AnimationActorKey.Action.Idle, true);
            }

            AudioManager.Instance.PlayOneShot("SFX", "SFX_Dialogue_Call");

            var instance = DialogueController.Instance;
            instance.ResetDialogue();
            instance.Visible = true;
            instance.SetDisplayName(nameKey?.ActorKey);

            if (nameKey is not null && ActorDataManager.Instance.CachedDict.TryGetValue(nameKey.ActorKey, out var data))
            {
                instance.SetPortrait(data.ActorKey, data.DefaultPortraitKey);
            }

            var resultType = await instance.GetBranchResultAsync(dialogueEvent.Type);

            if (resultType is DialogueBranchType.Dialogue)
            {
                _ = await RunDialogue(dialogueEvent.Container, dialogueEvent.ProcessorData);
            }

            stateTransfer?.TranslateState("DailyRoutine");
            return true;
        }


        return false;
    }

    public CollisionInteractionMono FindCloserObject()
    {
        return _controller.Interactor.CloserObject;
        
       // var targetPos = _controller.Coordinate.GetFront();
       // var colliders =
       //     Physics2D.OverlapCircleAll(targetPos, _controller.InteractionRadius, ~LayerMask.GetMask("Player"));
//
       // float minDis = Mathf.Infinity;
       // CollisionInteractionMono minInteraction = null;
       // foreach (var col in colliders)
       // {
       //     if (col.TryGetComponent(out CollisionInteractionMono interaction)
       //        )
       //     {
       //         float dis = (transform.position - col.transform.position).sqrMagnitude;
       //         if (dis < minDis)
       //         {
       //             minInteraction = interaction;
       //             minDis = dis;
       //         }
       //     }
       // }
//
       // return minInteraction;
    }
}