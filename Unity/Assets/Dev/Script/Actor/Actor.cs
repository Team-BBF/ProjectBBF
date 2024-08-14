using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;

public class Actor : MonoBehaviour, IBANameKey
{
    [field: SerializeField, Foldout("데이터"), MustBeAssigned, InitializationField]
    private string _actorKey;
    [field: SerializeField, Foldout("데이터")] private AnimationData _aniData;
    [field: SerializeField, Foldout("데이터")] private ActorMovementData _movementData;

    [field: SerializeField, Foldout("컴포넌트"), AutoProperty, MustBeAssigned, InitializationField]
    private CollisionInteraction _interaction;

    [field: SerializeField, Foldout("컴포넌트")] private StateTransitionHandler _transitionHandler;
    [field: SerializeField, Foldout("컴포넌트")] private Animator _animator;
    [field: SerializeField, Foldout("컴포넌트")] private Rigidbody2D _rigid;
    
    [field: SerializeField, Foldout("월드 데이터")] private PatrolPointPath _patrolPath;
    
    

    #region Getter/Setter
    public ActorMovementData MovementData => _movementData;
    public AnimationData AniData => _aniData;

    public Rigidbody2D Rigid => _rigid;

    public Animator Animator => _animator;

    public PatrolPointPath PatrolPath => _patrolPath;

    public CollisionInteraction Interaction => _interaction;

    public string ActorKey => _actorKey;
    #endregion

    #region Strategy
    public ActorMove MoveStrategy { get; private set; }
    public ActorVisual Visual { get; private set; }
    public ActorFavorablity Favorablity { get; private set; } 
    #endregion
    
    private void Awake()
    {
        //* Strategy binding */
        MoveStrategy = gameObject.AddComponent<ActorMove>();
        MoveStrategy.Init(this);
        Visual = gameObject.AddComponent<ActorVisual>();
        Visual.Init(Animator, AniData);
        Favorablity = gameObject.AddComponent<ActorFavorablity>();
        Favorablity.Init(this);
        
        
        /* Collision interaction */
        var info = ActorContractInfo.Create(() => gameObject);
        info.AddBehaivour<IBAFavorablity>(Favorablity);
        info.AddBehaivour<IBANameKey>(this);
        info.AddBehaivour<IBAStateTransfer>(_transitionHandler);
        
        _interaction.SetContractInfo(info, this);
        
        
        /* State handler */
        _transitionHandler.Init(Interaction);
        _transitionHandler.AddHandleCallback("DailyRoutine", ToDailyRoutine);
        _transitionHandler.AddHandleCallback("TalkingForPlayer", ToTalkingForPlayer);
    }


    #region StatehandleCallback
    private bool ToDailyRoutine()
    {
        return false;
    }
    private bool ToTalkingForPlayer()
    {
        return false;
    }

    #endregion
}
