using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Event;
using ProjectBBF.Input;
using ProjectBBF.Persistence;
using UnityEngine;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously


public class PlayerInteracter : MonoBehaviour, IPlayerStrategy
{
    /** 컨트롤 필드 */
    private PlayerController _controller;

    private ActorVisual _visual;
    private PlayerBlackboard _blackboard;
    private PlayerMove _move;
    private PlayerCoordinate _coordinate;
    private SpriteRenderer _indicator;
    private SpriteRenderer _itemPreviewRenderer;

    /** Interaction 필드 */
    private List<CollisionInteractionMono> _closerObjects = new(5);

    public CollisionInteractionMono CloserObject { get; private set; }
    public IReadOnlyList<CollisionInteractionMono> CloserObjects => _closerObjects;
    public event Action<CollisionInteractionMono> OnChangedCloserObject;
    public Vector2 IndicatedPosition => _indicator.transform.position;
    public bool IsInteracting { get; set; }

    public void Init(PlayerController controller)
    {
        _controller = controller;
        _visual = controller.VisualStrategy;
        _move = controller.MoveStrategy;
        _coordinate = controller.Coordinate;
        _indicator = controller.InteractorIndicator;
        _indicator.enabled = false;
        _itemPreviewRenderer = controller.ItemPreviewRenderer;
        _blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");

        ItemPreviewSprite = null;
    }

    #region Properties

    public bool MainInventoryVisible
    {
        get => _controller.Inventory.MainInvVisible;
        set => _controller.Inventory.MainInvVisible = value;
    }

    public bool QuickInventoryVisible
    {
        get => _controller.Inventory.QuickInvVisible;
        set => _controller.Inventory.QuickInvVisible = value;
    }

    public Sprite ItemPreviewSprite
    {
        get => _itemPreviewRenderer.sprite;
        set
        {
            _itemPreviewRenderer.enabled = value;
            _itemPreviewRenderer.sprite = value;
        }
    }

    #endregion

    #region Wait Methods

    public async UniTask WaitForSecondAsync(float sec, CancellationToken token = default)
    {
        token = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy()).Token;

        _move.ResetVelocity();
        IsInteracting = true;

        _ = await UniTask
            .Delay((int)(sec * 1000f), DelayType.DeltaTime, PlayerLoopTiming.Update, token)
            .SuppressCancellationThrow();

        IsInteracting = false;
    }

    public void WaitForSecond(float sec)
    {
        _ = WaitForSecondAsync(sec);
    }

    public void WaitForDefault()
    {
        WaitForSecond(0.3f);
    }
    public void WaitForPickupAnimation(Vector2 dir)
    {
        WaitForSecond(0.8f);
        _visual.SetAction(AnimationActorKey.Action.Collect, dir);
    }

    #endregion

    #region Public Method

    public void AddCloserObject(CollisionInteractionMono interaction)
    {
        _closerObjects.Add(interaction);
    }

    public void RemoveCloserObject(CollisionInteractionMono interaction)
    {
        _closerObjects.Remove(interaction);
    }

    public bool ContainsCloserObject(CollisionInteractionMono interaction)
    {
        return _closerObjects.Contains(interaction);
    }

    #endregion

    #region Private Method

    private void CalcultateIndicatorPosition()
    {
        ItemData currentData = _controller.Inventory.CurrentItemData;

        if (currentData && (
                currentData.Info.Contains(ToolType.Hoe) ||
                currentData.Info.Contains(ToolType.Hammer) ||
                currentData.Info.Contains(ToolType.Sickle) ||
                currentData.Info.Contains(ToolType.WaterSpray) ||
                currentData.Info.Contains(ToolType.Fertilizer) ||
                currentData.Info.Contains(ToolType.Seed) ||
                currentData.Info.Contains(ToolType.Pickaxe)
            ))
        {
            var obj = FindCloserObject();
            if (obj == false) return;

            var pos = _coordinate.GetLookAtPosition();

            if (obj.TryGetContractInfo(out ObjectContractInfo info) &&
                info.TryGetBehaviour(out IBOInteractIndicator interactIndicator) &&
                interactIndicator.CanDraw(pos))
            {
                _indicator.enabled = true;
                _indicator.transform.position = interactIndicator.GetDrawPositionAndSize(pos).position;
                return;
            }
        }

        _indicator.enabled = false;
    }

    private void CalculateCloseObject()
    {
        float minDis = Mathf.Infinity;
        CollisionInteractionMono minObj = null;

        int nullCount = 0;

        foreach (CollisionInteractionMono obj in _closerObjects)
        {
            if (obj == false)
            {
                nullCount++;
                continue;
            }

            float dis = ((Vector2)(obj.transform.position - _controller.transform.position)).sqrMagnitude;

            if (dis < minDis)
            {
                minDis = dis;
                minObj = obj;
            }
        }

        if (CloserObject != minObj)
        {
            CloserObject = minObj;
            OnChangedCloserObject?.Invoke(CloserObject);
        }

        if (nullCount >= 10)
        {
            _closerObjects.RemoveAll(x => x == false);
        }
    }

    private void PlayAudio(ItemData itemData, string usingKey)
    {
        if (itemData == false) return;

        if (itemData.UseActionUsingActionAudioInfos is null) return;

        foreach (var info in itemData.UseActionUsingActionAudioInfos)
        {
            if (info.HasAudio(usingKey))
            {
                AudioManager.Instance.PlayOneShot(info.MixerGroupKey, info.AudioKey);
            }
        }
    }

    public CollisionInteractionMono FindCloserObject()
    {
        var targetPos = _controller.Coordinate.GetLookAtPosition();
        var colliders =
            Physics2D.OverlapCircleAll(targetPos, _controller.CoordinateData.Radius,
                ~LayerMask.GetMask("Player", "Ignore Raycast"));

        float minDis = Mathf.Infinity;
        CollisionInteractionMono minInteraction = null;
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out CollisionInteractionMono interaction)
               )
            {
                float dis = (transform.position - col.transform.position).sqrMagnitude;
                if (dis < minDis)
                {
                    minInteraction = interaction;
                    minDis = dis;
                }
            }
        }

        return minInteraction;
    }

    public CollisionInteractionMono FindClickObject()
    {
        if (InputManager.Map.Player.InteractionMouseClick.triggered)
        {
            return FindClickObjectWithoutClick();
        }

        return null;
    }

    public CollisionInteractionMono FindClickObjectWithoutClick()
    {
        Vector2 mousePos = InputManager.Map.Player.Look.ReadValue<Vector2>();
        Camera camera = Camera.main;
        if (camera)
        {
            Vector2 worldPos = camera.ScreenToWorldPoint(mousePos);

            Collider2D[] cols = Physics2D.OverlapCircleAll(worldPos, 0.15f, ~LayerMask.GetMask("Player", "Ignore Raycast"));

            float minDis = Mathf.Infinity;
            CollisionInteractionMono minObj = null;
            
            foreach (Collider2D col in cols)
            {
                if (col && col.TryGetComponent(out CollisionInteractionMono interaction))
                {
                    float dis = ((Vector2)(col.transform.position - _controller.transform.position)).sqrMagnitude;
                    if (dis < minDis)
                    {
                        minDis = dis;
                        minObj = interaction;
                    }
                }
            }
            
            return minObj;
        }

        return null;
    }

    #endregion

    #region Unity Method

    private void Update()
    {
        CalculateCloseObject();
        CalcultateIndicatorPosition();

        if (ScreenManager.Instance)
        {
            if (FindClickObjectWithoutClick())
            {
                ScreenManager.Instance.CurrentCursor = CursorType.CanClick;                
            }
            else
            {
                ScreenManager.Instance.CurrentCursor = CursorType.Default;
            }
        }
    }

    #endregion
}