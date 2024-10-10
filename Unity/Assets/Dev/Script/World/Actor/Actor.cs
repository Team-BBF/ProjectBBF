using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(CollisionInteraction))]
public class Actor : MonoBehaviour, IBANameKey
{
    [field: SerializeField, Foldout("데이터"), MustBeAssigned, InitializationField]
    private string _actorKey;
    [field: SerializeField, Foldout("데이터")] private ActorMovementData _movementData;

    
    
    [field: SerializeField, Foldout("컴포넌트")] private List<ActorComponent> _actorComponents;

    [field: SerializeField, Foldout("컴포넌트"), AutoProperty, MustBeAssigned, InitializationField]
    private CollisionInteraction _interaction;

    [field: SerializeField, Foldout("컴포넌트")] private StateTransitionHandler _transitionHandler;
    [field: SerializeField, Foldout("컴포넌트")] private Animator _animator;
    [field: SerializeField, Foldout("컴포넌트")] private Rigidbody2D _rigid;
    [field: SerializeField, Foldout("컴포넌트")] private NavMeshAgent _agent;
    

    #region Getter/Setter
    public ActorMovementData MovementData => _movementData;
    public Rigidbody2D Rigid => _rigid;

    public Animator Animator => _animator;

    public PatrolPointPath PatrolPath { get; private set; }

    public CollisionInteraction Interaction => _interaction;

    public string ActorKey => _actorKey;
    #endregion

    #region Strategy
    public ActorMove MoveStrategy { get; private set; }
    public ActorVisual Visual { get; private set; }
    public ActorFavorablity Favorablity { get; private set; }

    public StateTransitionHandler TransitionHandler => _transitionHandler;

    #endregion
    
    protected virtual void Awake()
    {
        //* Strategy binding */
        MoveStrategy = gameObject.AddComponent<ActorMove>();
        MoveStrategy.Init(this);
        Visual = gameObject.AddComponent<ActorVisual>();
        Visual.Init(Animator, GetComponentInChildren<SpriteRenderer>());
        Favorablity = gameObject.AddComponent<ActorFavorablity>();
        Favorablity.Init(this);
        
        
        /* Collision interaction */
        var info = ActorContractInfo.Create(() => gameObject);
        info.AddBehaivour<IBADialogue>(Favorablity);
        info.AddBehaivour<IBANameKey>(this);
        info.AddBehaivour<IBAStateTransfer>(_transitionHandler);
        
        _interaction.SetContractInfo(info, this);
        
        
        /* State handler */
        _transitionHandler.Init(Interaction);

        StartCoroutine(CoPathEvent());
        
        GameObjectStorage.Instance.AddGameObject(gameObject);

        foreach (var com in _actorComponents)
        {
            if (com)
            {
                com.Init(this);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameObjectStorage.Instance == false) return;
        GameObjectStorage.Instance.RemoveGameObject(gameObject);
    }

    #region Private

    private IEnumerator CoPathEvent()
    {
        List<ActorMovementData.PathItem> list = new(_movementData.Paths);
        
        while (list.Any())
        {
            int index = -1;
            while (index == -1)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].ChangeTimeEvent.IsTriggered)
                    {
                        index = i;
                        break;
                    }
                }

                yield return null;
            }

            CalculatePath(list[index].Path.GetComponent<PatrolPointPath>());
            
            list.RemoveAt(index);
            MoveStrategy.ResetMove();
            yield return null;
        }
    }

    private PatrolPointPath _tempPatrolPointPath;
    private void CalculatePath(PatrolPointPath currentPath)
    {
        float minDis = Mathf.Infinity;
        int index = -1;
        for (int i = 0; i < currentPath.PatrollPoints.Count; i++)
        {
            PatrolPoint point = currentPath.PatrollPoints[i];
            if (point.InteractiveDecoratedPoint?.Teleport ?? false)
            {
                index = i;
                break;
            }
                
            NavMeshPath navPath = new NavMeshPath();
            _agent.CalculatePath(point.InteractiveDecoratedPoint?.InteractingPosition ?? point.Position, navPath);

            if (navPath.status == NavMeshPathStatus.PathInvalid)
            {
                continue;
            }
            
            float dis = GetLengthSqrt(navPath);
            if (dis < minDis)
            {
                minDis = dis;
                index = i;
            }
        }

        if (_tempPatrolPointPath == false)
        {
            _tempPatrolPointPath = gameObject.AddComponent<PatrolPointPath>();
        }

        var path = currentPath.PatrollPoints.ToList().GetRange(index, currentPath.PatrollPoints.Count - index);
        
        _tempPatrolPointPath.SetPatrollPoints(path);
        PatrolPath = _tempPatrolPointPath;

    }
    public static float GetLengthSqrt(NavMeshPath path)
    {
        var corners = path.corners;
        float length = 0;
        for (var i = 1; i < corners.Length; i++)
        {
            length += (corners[i - 1] - corners[i]).sqrMagnitude;
        }

        return length;
    }
    #endregion
}
