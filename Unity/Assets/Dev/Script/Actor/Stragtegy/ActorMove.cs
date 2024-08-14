using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;

public class ActorMove : MonoBehaviour, IActorStrategy
{
    private Rigidbody2D _rigid;
    private ActorMovementData _data;
    private Actor _actor;

    private CancellationTokenSource _moveCancel;
    
    public CollisionInteraction Interaction { get; private set; }
    public void Init(Actor actor)
    {
        _rigid = actor.Rigid;
        _data = actor.MovementData;
        _actor = actor;
        Interaction = actor.Interaction;
        
        _moveCancel = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancelation.PlayMode);
    }

    public void ResetMove()
    {
        _moveCancel?.Cancel();
        _moveCancel = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancelation.PlayMode);
    }
    
    public async UniTask<Vector2> MoveToPoint(Vector2 pos)
    {
        await UniTask.WaitUntil(() =>
        {
            _rigid.position = Vector2.MoveTowards(_rigid.position, pos, Time.deltaTime * _data.MovementSpeed);
            _actor.Visual.LookAt(pos - _rigid.position, false);
            
            return Vector2.Distance(_rigid.position, pos) < 0.001f;
        }, PlayerLoopTiming.Update, _moveCancel.Token);

        return pos;
    }

}