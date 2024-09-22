using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.AI;

public class ActorMove : MonoBehaviour, IActorStrategy
{
    private NavMeshAgent _agent;
    private ActorMovementData _data;
    private Actor _actor;

    private CancellationTokenSource _moveCancel;
    
    public CollisionInteraction Interaction { get; private set; }
    public void Init(Actor actor)
    {
        _agent = actor.GetComponent<NavMeshAgent>();
        _data = actor.MovementData;
        _actor = actor;
        Interaction = actor.Interaction;

        UniTask.Create(async () =>
        {
            while (true)
            {
                if (_agent.isOnOffMeshLink)
                {
                    _agent.speed = _data.MovementSpeed * 0.45f;
                }
                else
                {
                    _agent.speed = _data.MovementSpeed;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }
        }).Forget();
        
        _moveCancel = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    public void ResetMove()
    {
        _moveCancel?.Cancel();
        _moveCancel = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }
    
    public async UniTask<Vector2> MoveToPoint(PatrolPoint point)
    {
        if(point is null)return transform.position;
        
        bool backupVisible = _actor.Visual.IsVisible;
        try
        {
            while (true)
            {
                if (TimeManager.Instance is not null && TimeManager.Instance.IsRunning is false)
                {
                    _agent.isStopped = true;
                    await UniTask.Yield(PlayerLoopTiming.Update, _moveCancel.Token);
                    continue;
                }

                _agent.isStopped = false;

                Vector3 pos = point.Position;
                if (point.InteractiveDecoratedPoint)
                {
                    pos = point.InteractiveDecoratedPoint.InteractingPosition;
                    if (point.InteractiveDecoratedPoint.Teleport)
                    {
                        _agent.transform.position = (Vector2)point.Position;
                        _agent.SetDestination(point.Position);
                        _actor.Visual.LookAt(_agent.desiredVelocity, AnimationActorKey.Movement.Idle);
                        break;
                    }
                }

                _agent.SetDestination(pos);
                _actor.Visual.LookAt(_agent.desiredVelocity, AnimationActorKey.Movement.Walk);

                if (Vector2.Distance(_agent.transform.position, pos) <= _agent.stoppingDistance)
                {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, _moveCancel.Token);
            }

            await InteractPoint(point);
            _actor.Visual.IsVisible = backupVisible;
            _agent.isStopped = false;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
        finally
        {
            if (_actor)
            {
                _actor.Visual.IsVisible = backupVisible;
            }
        }
        
        return point.Position;
    }

    private async UniTask InteractPoint(PatrolPoint point)
    {
        var decoPoint = point.InteractiveDecoratedPoint;
        if (decoPoint == false) return;

        _actor.Visual.IsVisible = !decoPoint.VisitAndHide;

        _actor.Visual.LookAt(_agent.desiredVelocity, AnimationActorKey.Movement.Idle);
        float timer = 0f;
        while (timer < decoPoint.WaitDuration)
        {
            if (TimeManager.Instance is not null && TimeManager.Instance.IsRunning is false)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, _moveCancel.Token);
                continue;
            }

            timer += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, _moveCancel.Token);
        }


    }

}