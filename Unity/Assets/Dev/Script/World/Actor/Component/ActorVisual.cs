using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ActorVisual : ActorComponent
{
    private static readonly int MoveSpeedAniHash = Animator.StringToHash("MoveSpeed");
    private Animator _animator;
    private SpriteRenderer _renderer;
    private static readonly int ActionAniHash = Animator.StringToHash("Action");
    private static readonly int ForceMovementAniHash = Animator.StringToHash("ForceMovement");

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

    public Vector2 MoveDir
    {
        get => new Vector2(_animator.GetFloat("MoveX"), _animator.GetFloat("MoveY"));
        set
        {
            AnimationActorKey.Direction dir = VectorToDirection(value);
            SetLookAtRight(dir);
            
            _animator.SetFloat("MoveX", value.x);
            _animator.SetFloat("MoveY", value.y);
        }
    }
    
    public float MoveSqrt
    {
        get => _animator.GetFloat("MoveSqrt");
        set => _animator.SetFloat("MoveSqrt", value);
    }

    public float MoveSpeed
    {
        get => _animator.GetFloat(MoveSpeedAniHash);
        set => _animator.SetFloat(MoveSpeedAniHash, value);
    }
    
    public virtual void SetAction(AnimationActorKey.Action action)
    {
        int aniHash = AnimationActorKey.GetAniHash(action);
        _animator.SetTrigger(aniHash);
        _animator.SetTrigger(ActionAniHash);
    }
    public virtual void SetAction(AnimationActorKey.Action action, Vector2 vDir, bool ignoreSideUp = false)
    {
        var dir = VectorToDirection(vDir, ignoreSideUp);
        SetAction(action, dir);
    }
    public virtual void SetAction(AnimationActorKey.Action action, AnimationActorKey.Direction dir, bool ignoreSideUp = false)
    {
        int aniHash = AnimationActorKey.GetAniHash(action);
        _animator.SetTrigger(aniHash);
        _animator.SetTrigger(ActionAniHash);
        MoveDir = DirectionToVector(dir, ignoreSideUp);
    }

    public void SetIdle(AnimationActorKey.Direction dir)
    {
        MoveDir = DirectionToVector(dir);
        MoveSqrt = 0f;
        _animator.SetTrigger("ForceMovement");
        _animator.ResetTrigger(ActionAniHash);
    }
    public void SetIdle(Vector2 dir)
    {
        MoveDir = dir;
        MoveSqrt = 0f;
        _animator.SetTrigger("ForceMovement");
        _animator.ResetTrigger(ActionAniHash);
    }
    public void SetMove(AnimationActorKey.Direction dir)
    {
        MoveDir = DirectionToVector(dir);
        MoveSqrt = 1f;
        _animator.SetTrigger("ForceMovement");
        _animator.ResetTrigger(ActionAniHash);
    }
    public void SetMove(Vector2 dir)
    {
        MoveDir = dir;
        MoveSqrt = 1f;
        _animator.SetTrigger(ForceMovementAniHash);
        _animator.ResetTrigger(ActionAniHash);
    }
    
    public void SetLookAtRight(AnimationActorKey.Direction direction)
    {
        if (AnimationActorKey.Direction.Up == direction)
        {
            IsRendererLookAtRight = true;
        }
        else if (AnimationActorKey.Direction.Down == direction)
        {
            IsRendererLookAtRight = true;
        }
        else if (AnimationActorKey.Direction.Left == direction)
        {
            IsRendererLookAtRight = false;
        }
        else if (AnimationActorKey.Direction.LeftUp == direction)
        {
            IsRendererLookAtRight = false;
        }
        else if (AnimationActorKey.Direction.Right == direction)
        {
            IsRendererLookAtRight = true;
        }
        else if (AnimationActorKey.Direction.RightUp == direction)
        {
            IsRendererLookAtRight = true;
        }
    }

    public AnimationActorKey.Direction VectorToDirection(Vector2 direction, bool ignoreSideUp = false)
    {
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }
        
        // up
        if (ContainsDirection(direction, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            return AnimationActorKey.Direction.Up;
        }
        // left
        else if (ContainsDirection(direction, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            return AnimationActorKey.Direction.Left;
        }
        // leftup
        else if (ContainsDirection(direction, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            return ignoreSideUp ? AnimationActorKey.Direction.Left : AnimationActorKey.Direction.LeftUp;
        }
        // right
        else if (ContainsDirection(direction, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            return AnimationActorKey.Direction.Right;
        }
        // rightUp
        else if (ContainsDirection(direction, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            return ignoreSideUp ? AnimationActorKey.Direction.Right : AnimationActorKey.Direction.RightUp;
        }
        // down
        else if (ContainsDirection(direction, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            return AnimationActorKey.Direction.Down;
        }

        return AnimationActorKey.Direction.Down;
    }

    public Vector2 DirectionToVector(AnimationActorKey.Direction dir, bool ignoreSideUp = false)
    {
        // up
        if (dir == AnimationActorKey.Direction.Up)
        {
            return Vector2.up;
        }
        // left
        else if (dir == AnimationActorKey.Direction.Left)
        {
            return Vector2.left;
        }
        // leftup
        else if (dir == AnimationActorKey.Direction.LeftUp)
        {
            return ignoreSideUp ? Vector2.left : Vector2.left + Vector2.up;
        }
        // right
        else if (dir == AnimationActorKey.Direction.Right)
        {
            return Vector2.right;
        }
        // rightUp
        else if (dir == AnimationActorKey.Direction.RightUp)
        {
            return ignoreSideUp ? Vector2.right : Vector2.right + Vector2.up;
        }
        // down
        else if (dir == AnimationActorKey.Direction.Down)
        {
            return Vector2.down;
        }

        return Vector2.down;
    }
    public Vector2 ToNormalizedVector(Vector2 direction, bool ignoreSideUp = false)
    {
        AnimationActorKey.Direction dir = VectorToDirection(direction, ignoreSideUp);

        return DirectionToVector(dir, ignoreSideUp);
    }
}