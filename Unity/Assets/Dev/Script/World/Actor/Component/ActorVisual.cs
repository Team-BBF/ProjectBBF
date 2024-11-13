using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ActorVisual : ActorComponent
{
    [SerializeField] private bool _useFlipLeftRenderer = true;
    
    private static readonly int MoveSpeedAniHash = Animator.StringToHash("MoveSpeed");
    private Animator _animator;
    private SpriteRenderer _renderer;
    private IActorMove _move;

    private int _beforeActionHash;
    private int _beforeDirectionHash;

    public Animator Animator => _animator;

    public bool IsVisible
    {
        get => _renderer.enabled;
        set => _renderer.enabled = value;
    }

    private bool IsRendererLookAtRight
    {
        get => _renderer.flipX is false;
        set => _renderer.flipX = !value;
    }

    public RuntimeAnimatorController RuntimeAnimator
    {
        get => _animator.runtimeAnimatorController;
        set => _animator.runtimeAnimatorController = value;
    }


    public void Init(IActorMove move, Animator animator, SpriteRenderer renderer)
    {
        _move = move;
        _animator = animator;
        _renderer = renderer;
    }

    private bool ContainsDirection(Vector2 targetDir, Vector2 dir, float angle)
    {
        targetDir = targetDir.normalized;
        dir = dir.normalized;

        return Mathf.Acos(Vector2.Dot(targetDir, dir)) * Mathf.Rad2Deg <= angle;
    }

    public void ClearState()
    {
        _beforeActionHash = 0;
        _beforeDirectionHash = 0;
    }
    
    public virtual void ChangeClip(int actionAniHash, int directionAniHash, bool force = false)
    {
        if (actionAniHash == -1 || directionAniHash == -1) return;
        
        if (force is false && _beforeActionHash == actionAniHash && _beforeDirectionHash == directionAniHash) return;

        _beforeActionHash = actionAniHash;
        _beforeDirectionHash = directionAniHash;
        _animator.SetTrigger(actionAniHash);
        _animator.SetTrigger(directionAniHash);
        SetLookAtRight(directionAniHash);
    }
    public virtual void ChangeClip((int actionAniHash, int directionAniHash) tuple, bool force = false)
    {
        ChangeClip(tuple.actionAniHash, tuple.directionAniHash, force);
    }

    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat(MoveSpeedAniHash, speed);
    }
    
    private void SetLookAtRight(int aniHash)
    {
        if (AnimationActorKey.GetAniHash(AnimationActorKey.Direction.Up) == aniHash)
        {
            IsRendererLookAtRight = true;
        }
        else if (AnimationActorKey.GetAniHash(AnimationActorKey.Direction.Down) == aniHash)
        {
            IsRendererLookAtRight = true;
        }
        else if (AnimationActorKey.GetAniHash(AnimationActorKey.Direction.Left) == aniHash)
        {
            IsRendererLookAtRight = false;
        }
        else if (AnimationActorKey.GetAniHash(AnimationActorKey.Direction.LeftUp) == aniHash)
        {
            IsRendererLookAtRight = false;
        }
        else if (AnimationActorKey.GetAniHash(AnimationActorKey.Direction.Right) == aniHash)
        {
            IsRendererLookAtRight = true;
        }
        else if (AnimationActorKey.GetAniHash(AnimationActorKey.Direction.RightUp) == aniHash)
        {
            IsRendererLookAtRight = true;
        }
        else
        {
            Debug.Assert(false, "잘못된 Direction Hash: " + aniHash);
        }
    }

    public Vector2 CalculateLookDir(Vector2 toTargetDir, bool ignoreSideUp = false)
    {
        if (toTargetDir.sqrMagnitude > 1f)
        {
            toTargetDir.Normalize();
        }
        
        // up
        if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            return Vector2.up;
        }
        // left
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            return Vector2.left;
        }
        // leftup
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            return ignoreSideUp ? Vector2.left : (Vector2.left + Vector2.up).normalized;
        }
        // right
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            return Vector2.right;
        }
        // rightUp
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            return ignoreSideUp ? Vector2.right : (Vector2.right + Vector2.up).normalized;
        }
        // down
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            return Vector2.down;
        }

        return Vector2.down;
    }
    
    public void LookAt(Vector2 toTargetDir, AnimationActorKey.Action movementType, bool ignoreSideUp = false)
    {
        int? actorAniHash = null;
        int? directionAniHash = null;

        if (toTargetDir.sqrMagnitude > 1f)
        {
            toTargetDir.Normalize();
        }
        
        // up
        if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Up);
            _move.LastMovedDirection = Vector2.up;
        }
        // left
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Left);
            _move.LastMovedDirection = Vector2.left;
        }
        // leftup
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash =
                ignoreSideUp is false
                    ? AnimationActorKey.GetAniHash(AnimationActorKey.Direction.LeftUp)
                    : AnimationActorKey.GetAniHash(AnimationActorKey.Direction.Left);
            
            
            _move.LastMovedDirection = ignoreSideUp ? Vector2.left : (Vector2.left + Vector2.up).normalized;

        }
        // right
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Right);
            _move.LastMovedDirection = Vector2.right;
        }
        // rightUp
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash =
                ignoreSideUp is false
                    ? AnimationActorKey.GetAniHash(AnimationActorKey.Direction.RightUp)
                    : AnimationActorKey.GetAniHash(AnimationActorKey.Direction.Right);
            
            _move.LastMovedDirection = ignoreSideUp ? Vector2.right : (Vector2.right + Vector2.up).normalized;
        }
        // down
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            actorAniHash = AnimationActorKey.GetAniHash(movementType);
            directionAniHash = AnimationActorKey.GetAniHash( AnimationActorKey.Direction.Down);
            _move.LastMovedDirection = Vector2.down;
        }

        if (actorAniHash is null) return;


        SetLookAtRight(directionAniHash.Value);
        ChangeClip(actorAniHash.Value, directionAniHash.Value);
    }
}