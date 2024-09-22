using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class ActorVisual : MonoBehaviour, IActorStrategy
{
    private Animator _animator;
    private SpriteRenderer _renderer;

    private int _beforeAniHash = -1;

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
    
    public virtual void ChangeClip(int aniHash, bool force = false)
    {
        if (force is false && _beforeAniHash == aniHash) return;

        _beforeAniHash = aniHash;
        _animator.SetTrigger(aniHash);
    }

    private bool ContainsDirection(Vector2 targetDir, Vector2 dir, float angle)
    {
        targetDir = targetDir.normalized;
        dir = dir.normalized;

        return Mathf.Acos(Vector2.Dot(targetDir, dir)) * Mathf.Rad2Deg <= angle;
    }
    
    public void LookAt(Vector2 toTargetDir, AnimationActorKey.Movement movementType)
    {
        int? aniHash = null;

        // up
        if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 90f) * Vector2.right, 30f))
        {
            aniHash = AnimationActorKey.GetAniHash(movementType, AnimationActorKey.Direction.Up);
        }
        // left
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 25f) * Vector2.left, 30f))
        {
            aniHash = AnimationActorKey.GetAniHash(movementType, AnimationActorKey.Direction.Left);
        }
        // leftup
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -30f) * Vector2.left, 30f))
        {
            aniHash = AnimationActorKey.GetAniHash(movementType, AnimationActorKey.Direction.LeftUp);
        }
        // right
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -25f) * Vector2.right, 30f))
        {
            aniHash = AnimationActorKey.GetAniHash(movementType, AnimationActorKey.Direction.Right);
        }
        // rightUp
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, 30f) * Vector2.right, 30f))
        {
            aniHash = AnimationActorKey.GetAniHash(movementType, AnimationActorKey.Direction.RightUp);
        }
        // down
        else if (ContainsDirection(toTargetDir, Quaternion.Euler(0f, 0f, -90f) * Vector2.right, 30f))
        {
            aniHash = AnimationActorKey.GetAniHash(movementType, AnimationActorKey.Direction.Down);
        }

        if (aniHash is null) return;

        ChangeClip(aniHash.Value);
    }
}