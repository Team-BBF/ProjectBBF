using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;


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

        PathEvent().Forget();
        
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

    private async UniTask PathEvent()
    {
        HashSet<ESOGameTimeEvent> _invokedEvents = new();
        List<UniTask<PatrolPointPath>> list = new List<UniTask<PatrolPointPath>>(_movementData.Paths.Count);

        while (true)
        {
            try
            {
                list.Clear();

                foreach (ActorMovementData.PathItem item in _movementData.Paths)
                {
                    if (_invokedEvents.Contains(item.ChangeTimeEvent)) continue;

                    var i = item;
                    if(i.ChangeTimeEvent == false)continue;
                    if(i.ChangeTimeEvent.IsTriggered)continue;
                    
                    list.Add(UniTask.Create(async () =>
                    {
                        await i.ChangeTimeEvent.WaitAsync(this.GetCancellationTokenOnDestroy());
                        return i.Path.GetComponent<PatrolPointPath>();
                    }));
                }

                if(list.Any() is false)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
                    continue;
                }

                var path = await UniTask.WhenAny(list).WithCancellation(this.GetCancellationTokenOnDestroy());
                _invokedEvents.Add(_movementData.Paths[path.winArgumentIndex].ChangeTimeEvent);

                PatrolPath = path.result;
                MoveStrategy.ResetMove();
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Debug.LogException(e);
            }
        }
    }

    #endregion
}
