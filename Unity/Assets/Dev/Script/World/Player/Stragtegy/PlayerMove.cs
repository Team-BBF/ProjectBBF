using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour, IPlayerStrategy, IActorMove
{
    private InputAction _movementAction;
    private InputAction _sprintAction;
    private PlayerController _controller;
    private Rigidbody2D _rigidbody;
    private PlayerMovementData _movementData;
    private PlayerBlackboard _blackboard;

    
    public AnimationActorKey.Direction LastDirection { get; private set; }
    public AnimationActorKey.Action LastMovement { get; private set; }


    public Vector2 LastMovedDirection { get; set; }
    private Vector2 _lv;

    public Vector2 Velocity => _rigidbody.velocity;

    public bool IsStopped
    {
        get => _blackboard.IsMoveStopped;
        set => _blackboard.IsMoveStopped = value;
    }

    private bool _isGhost;
    public bool IsGhost
    {
        get => _rigidbody.GetComponent<Collider2D>().isTrigger;
        set => _rigidbody.GetComponent<Collider2D>().isTrigger = value;
    }
    
    public void Init(PlayerController controller)
    {
        _movementData = controller.MovementData;
        _rigidbody = controller.Rigidbody;
        _controller = controller;
        _blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
        
        BindInputAction();

        StartCoroutine(CoSteminaUpdate());
        
        
        LastDirection = AnimationActorKey.Direction.Down;
        LastMovement = AnimationActorKey.Action.Idle;
    }

    private void BindInputAction()
    {
        _movementAction = InputManager.Map.Player.Movement;
        _sprintAction = InputManager.Map.Player.Sprint;
    }

    private IEnumerator CoSteminaUpdate()
    {
        bool beforeSprint = false;
        while (true)
        {
            if (IsStopped)
            {
                yield return null;
                continue;
            }
            
            if (TimeManager.Instance.IsRunning is false)
            {
                yield return null;
                continue;
            }

            if (_sprintAction.IsPressed() is false && beforeSprint is false && _blackboard.Energy > 0)
            {
                _blackboard.Stemina += Time.deltaTime * _movementData.SteminaIncreasePerSec;
                yield return null;
                continue;
            }
            
            if (_sprintAction.IsPressed() && _blackboard.Stemina > 0f)
            {
                _blackboard.Stemina -= Time.deltaTime * _movementData.SteminaDecreasePerSec;
                beforeSprint = true;
                yield return null;
                continue;
            }

            float waitTimer = 0f;
            while (_sprintAction.IsPressed() is false && waitTimer < _movementData.SteminaIncreaseWaitDuration)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            beforeSprint = false;
            yield return null;
        }
    }

    private bool _inputFlag;
    public void Move(Vector2 input)
    {
        Vector2 dir = new Vector2(
            input.x,
            input.y
        );


        dir = dir.normalized;
        Vector2 velDir = Vector2.zero;
        
        if (_sprintAction.IsPressed() && _blackboard.Stemina > 0f)
        {
            velDir = dir * _movementData.SprintSpeed;
        }
        else
        {
            velDir = dir * _movementData.MovementSpeed;
        }

        _rigidbody.velocity = velDir;
        _controller.VisualStrategy.MoveSqrt = velDir.sqrMagnitude;
        
       
        // input이 없을 때
        if (Mathf.Approximately(Mathf.Abs(input.x) + Mathf.Abs(input.y), 0f))
        {
            if (_inputFlag is false)
            {
                _controller.VisualStrategy.MoveDir = _lv;
                _inputFlag = true;
            }
        }
        // input이 있을 때
        else 
        {
            _lv = dir;
            _controller.VisualStrategy.MoveDir = dir;
            _inputFlag = false;
        }
    }
    
    public void ResetVelocity()
    {
        _lv = Vector2.zero;
        _rigidbody.velocity = Vector3.zero;
    }
}
