using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ActorVisual : MonoBehaviour, IActorStrategy
{
    private Animator _animator;
    private SpriteRenderer _renderer;

    private int _beforeActionHash;
    private int _beforeDirectionHash;

    public bool IsVisible
    {
        get => _renderer.enabled;
        set => _renderer.enabled = value;
    }

    public void Init(Animator animator, SpriteRenderer renderer)
    {
        _animator = animator;
        _renderer = renderer;
    }

    private bool ContainsDirection(Vector2 targetDir, Vector2 dir, float angle)
    {
        targetDir = targetDir.normalized;
        dir = dir.normalized;

        return Mathf.Acos(Vector2.Dot(targetDir, dir)) * Mathf.Rad2Deg <= angle;
    }
    
    
    public virtual void ChangeClip(int actionAniHash, int directionAniHash, bool force = false)
    {
        if (force is false && _beforeActionHash == actionAniHash && _beforeDirectionHash == directionAniHash) return;

        _beforeActionHash = actionAniHash;
        _beforeDirectionHash = directionAniHash;
        _animator.SetTrigger(actionAniHash);
        _animator.SetTrigger(directionAniHash);
    }
    public virtual void ChangeClip((int actionAniHash, int directionAniHash) tuple, bool force = false)
    {
        ChangeClip(tuple.actionAniHash, tuple.directionAniHash, force);
    }
    
    public void LookAt(Vector2 toTargetDir, AnimationActorKey.Action movementType)
    {
        int? actorAniHash = null;
        int? directionAniHash = null;
        
        // up
        if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Up);
        }
        // left
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Left);
        }
        // leftup
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.LeftUp);
        }
        // right
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Right);
        }
        // rightUp
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.RightUp);
        }
        // down
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Down);
        }

        if (actorAniHash is null) return;

        ChangeClip(actorAniHash.Value, directionAniHash.Value);
    }
}