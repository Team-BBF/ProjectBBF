using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ActorVisual : MonoBehaviour, IActorStrategy
{
    private Animator _animator;
    private AnimationData _animationData;

    private const string DEFAULT_ANI_STATE = "DefaultMovement";

    private AnimationClip _defaultClip = null;
    private AnimationClip _beforeClip = null;

    public void Init(Animator animator, AnimationData animationData)
    {
        _animator = animator;
        _animationData = animationData;
        
        RuntimeAnimatorController ac = _animator.runtimeAnimatorController;
        AnimatorOverrideController overrideController = new AnimatorOverrideController(ac);
        _animator.runtimeAnimatorController = overrideController;

        _defaultClip = overrideController[DEFAULT_ANI_STATE];
    }
    
    public void ChangeClip(AnimationClip newClip, bool force = false)
    {
        if (force == false && _beforeClip == newClip) return;
        
        if (_animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
        {
            overrideController[_defaultClip] = newClip;
            _beforeClip = newClip;
        }
    }

    private bool ContainsDirection(Vector2 targetDir, Vector2 dir, float angle)
    {
        targetDir = targetDir.normalized;
        dir = dir.normalized;

        return Mathf.Acos(Vector2.Dot(targetDir, dir)) * Mathf.Rad2Deg <= angle;
    }
    
    public void LookAt(Vector2 toPlayerDir, bool isIdle)
    {
        // up
        if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            ChangeClip(isIdle ? _animationData.IdleUp : _animationData.MovementUp);
        }
        // left
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            ChangeClip(isIdle ? _animationData.IdleLeft : _animationData.MovementLeft);
        }
        // leftup
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            ChangeClip(isIdle ? _animationData.IdleLeftUp : _animationData.MovementLeftUp);
        }
        // right
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            ChangeClip(isIdle ? _animationData.IdleRight : _animationData.MovementRight);
        }
        // rightUp
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            ChangeClip(isIdle ? _animationData.IdleRightUp : _animationData.MovementRightUp);
        }
        // down
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            ChangeClip(isIdle ? _animationData.IdleDown : _animationData.MovementDown);
        }
    }
}