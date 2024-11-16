using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Event;
using ProjectBBF.Input;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class PlayerController : MonoBehaviour
{
    [field: SerializeField, Foldout("데이터"), OverrideLabel("플레이어 이동 데이터"), MustBeAssigned, DisplayInspector]
    private PlayerMovementData _movementData;
    [field: SerializeField, Foldout("데이터"), OverrideLabel("플레이어 이동 데이터"), MustBeAssigned, DisplayInspector]
    private PlayerCoordinateData _coordinateData;

    [field: SerializeField, Separator("컴포넌트"), MustBeAssigned, AutoProperty(AutoPropertyMode.Children)]
    private Rigidbody2D _rigidbody;

    [field: SerializeField, MustBeAssigned, AutoProperty(AutoPropertyMode.Children)]
    private Animator _animator;

    [field: SerializeField, MustBeAssigned, AutoProperty(AutoPropertyMode.Children)]
    private CollisionInteraction _interaction;

    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty(AutoPropertyMode.Scene)]
    private PlayerQuickInventoryView _quickInventoryView;

    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty(AutoPropertyMode.Scene)]
    private InteractableInventoryView _mainInventoryView;

    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty(AutoPropertyMode.Scene)]
    private PlayerPannelView pannelView;

    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty(AutoPropertyMode.Scene)]
    private RecipeBookPresenter _recipeBookPresenter;

    [field: SerializeField, MustBeAssigned, InitializationField]
    private RecipeBookPreviewView _recipeSummaryView;

    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty(AutoPropertyMode.Scene)]
    private QuestPresenter _questPresenter;
    
    [field: SerializeField, MustBeAssigned, InitializationField]
    private ParticlePlayer _hoeEffect;

    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty(AutoPropertyMode.Children)]
    private PlayerFishing _fishing;

    [field: SerializeField, MustBeAssigned, InitializationField]
    private SpriteRenderer _bodyRenderer;

    [field: SerializeField, MustBeAssigned, InitializationField]
    private SpriteRenderer _interactorIndicator;

    [field: SerializeField, MustBeAssigned, InitializationField]
    private SpriteRenderer _itemPreviewRenderer;

    [field: SerializeField] private List<ItemDataSerializedSet> _testItems;

    public CollisionInteraction Interaction => _interaction;
    public PlayerBlackboard Blackboard { get; private set; }
    private HudController _hudController;

    public HudController HudController
    {
        get
        {
            if (_hudController)
            {
                return _hudController;
            }

            GameObjectStorage.Instance?.ForEach(obj =>
            {
                if (obj.gameObject.TryGetComponent<HudController>(out var com))
                {
                    _hudController = com;
                    return false;
                }

                return true;
            });

            return _hudController;
        }
    }

    public RecipeBookPresenter RecipeBookPresenter => _recipeBookPresenter;

    public ParticlePlayer HoeEffect => _hoeEffect;


    #region Getter/Setter

    public PlayerMovementData MovementData => _movementData;

    public PlayerCoordinateData CoordinateData => _coordinateData;

    public Rigidbody2D Rigidbody => _rigidbody;
    public Animator Animator => _animator;

    public SpriteRenderer InteractorIndicator => _interactorIndicator;


    public PlayerMove MoveStrategy { get; private set; }
    public ActorVisual VisualStrategy { get; private set; }
    public PlayerStateTranslator Translator { get; private set; }
    public PlayerInteracter Interactor { get; private set; }
    public PlayerCoordinate Coordinate { get; private set; }
    public PlayerFishing Fishing => _fishing;
    public PlayerDialogue Dialogue { get; private set; }


    public PlayerInventoryPresenter Inventory { get; private set; }
    public PlayerPannelView PannelView => pannelView;

    public RecipeBookPreviewView RecipeSummaryView => _recipeSummaryView;

    public QuestPresenter QuestPresenter => _questPresenter;

    public SpriteRenderer ItemPreviewRenderer => _itemPreviewRenderer;
    
    public BasePlayerInputController InputController { get; private set; }

    #endregion
    private void Awake()
    {
        MoveStrategy = Bind<PlayerMove>();
        VisualStrategy = gameObject.AddComponent<ActorVisual>();
        Translator = Bind<PlayerStateTranslator>();
        Coordinate = Bind<PlayerCoordinate>();
        Interactor = Bind<PlayerInteracter>();
        Dialogue = Bind<PlayerDialogue>();
        VisualStrategy.Init(_animator, _bodyRenderer);

        GameObjectStorage.Instance.AddGameObject(gameObject);

        InputController = new DefaultBasePlayerInputController();
        InputController.Init(this);
        InputController.BindInput(InputAbstractFactory.CreateFactory<PlayerController, DefaultPlayerInputFactory>(this));

        var info = ObjectContractInfo.Create(() => gameObject);
        Interaction.SetContractInfo(info, this);
    }

    private void OnSaved(PlayerBlackboard blackboard)
    {
        blackboard.CurrentPosition = transform.position;
    }

    private void Start()
    {
        Blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
        Blackboard.OnSaved += OnSaved;

        GridInventoryModel model = Blackboard.CreateInventoryModelModel();
        
        Inventory = new PlayerInventoryPresenter(
            model,
            _mainInventoryView,
            _quickInventoryView,
            pannelView
        );

        if (model.GetSlotSequentially(0).Data == false)
        {
            foreach (var item in _testItems)
            {
                Inventory.Model.PushItem(item.Item, item.Count);
            }
        }   
        
        Inventory.Refresh();

        Fishing.Init(this);
        DataInit();
    }

    private void Update()
    {
        InputController.Update();
    }

    private void DataInit()
    {
        Blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");

        Blackboard.MaxStemina = _movementData.DefaultStemina;
        Blackboard.Stemina = _movementData.DefaultStemina;
        Blackboard.Inventory = Inventory;

        if (Blackboard.IsPositionSaved)
        {
           transform.position = Blackboard.CurrentPosition;
        }
    }

    private void OnDestroy()
    {
        if (GameObjectStorage.Instance)
        {
            GameObjectStorage.Instance.RemoveGameObject(gameObject);
        }
        
        InputController.Release();
        Blackboard.OnSaved -= OnSaved;
    }

    private T Bind<T>() where T : MonoBehaviour, IPlayerStrategy
    {
        var obj = gameObject.AddComponent<T>();
        obj.Init(this);

        return obj;
    }
}