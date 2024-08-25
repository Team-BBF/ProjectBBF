using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ActorVisual : MonoBehaviour, IActorStrategy
{
    private Animator _animator;
    private AnimationData _animationData;
    private SpriteRenderer _renderer;

    private const string DEFAULT_ANI_STATE = "DefaultMovement";

    private AnimationClip _defaultClip = null;
    private AnimationClip _beforeClip = null;

    public bool IsVisible
    {
        get => _renderer.enabled;
        set => _renderer.enabled = value;
    }

    public void Init(Animator animator, AnimationData animationData, SpriteRenderer renderer)
    {
        _animator = animator;
        _animationData = animationData;
        _renderer = renderer;
        
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
    
    public void LookAt(Vector2 toPlayerDir, AnimationData.Movement movementType)
    {
        AnimationClip clip = null;
        // up
        if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            clip = _animationData.GetClip(movementType, AnimationData.Direction.Up);
        }
        // left
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            clip = _animationData.GetClip(movementType, AnimationData.Direction.Left);
        }
        // leftup
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            clip = _animationData.GetClip(movementType, AnimationData.Direction.LeftUp);
        }
        // right
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            clip = _animationData.GetClip(movementType, AnimationData.Direction.Right);
        }
        // rightUp
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            clip = _animationData.GetClip(movementType, AnimationData.Direction.RightUp);
        }
        // down
        else if (ContainsDirection(toPlayerDir, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            clip = _animationData.GetClip(movementType, AnimationData.Direction.Down);
        }

        ChangeClip(clip);
    }
}