using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cinemachine.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyBox;
using ProjectBBF.Persistence;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

public class PlayerFishing : MonoBehaviour, IPlayerStrategy
{
    [Serializable]
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private SpriteRenderer _fishingStateRenderer;
    [SerializeField] private LineRenderer _line;
    [SerializeField] private float _horizontalDirAngle;
    [SerializeField] private float _verticalUpFactor;
    [SerializeField] private float _verticalDownFactor;
    [SerializeField] private float _speed;
    [SerializeField] private float _maxHandleInTagent;
    [SerializeField] private float _maxLineLength;
    [SerializeField] private float _beginLineLength;
    [SerializeField] private float _lineHeightFactor;

    [SerializeField] private int _lineIteraction = 20;

    [SerializeField] private Vector3 _sideOffset;


    [SerializeField] private float _fishingMaxDistance;
    [SerializeField] private float _turningT;
    [SerializeField] private float _sideMaxY;

    [SerializeField] private Transform _handle;
    [SerializeField] private FishingView _view;

    [SerializeField] private SplineContainer _splineContainer;

    private IEnumerator _co;

    private PlayerInventoryPresenter _invPresenter;
    private PlayerCoordinate _coordinate;
    private PlayerMove _move;
    private ActorVisual _visual;
    private PlayerBlackboard _blackboard;

    private bool _currenTurningT = false;

    public bool CanFishing
    {
        get
        {
            ItemData curItem = _invPresenter.CurrentItemData;
            if (curItem is null) return false;

            return curItem.Info.Contains(ToolType.FishingRod);
        }
    }

    public bool IsFishing { get; private set; }

    public void Init(PlayerController controller)
    {
        _invPresenter = controller.Inventory;
        _coordinate = controller.Coordinate;
        _move = controller.MoveStrategy;
        _visual = controller.VisualStrategy;

        _fishingStateRenderer.enabled = false;
        _blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
    }

    [ButtonMethod]
    private void LeftDirSet()
    {
        var front = _coordinate.GetFrontPureDir();
        var spline = GetCalculatedSpline(Direction.Left, transform.position + front * _fishingMaxDistance);
        _splineContainer.Spline = spline;
    }

    [ButtonMethod]
    private void RightDirSet()
    {
        var front = _coordinate.GetFrontPureDir();
        var spline = GetCalculatedSpline(Direction.Right, transform.position + front * _fishingMaxDistance);
        _splineContainer.Spline = spline;
    }

    [ButtonMethod]
    private void UpDirSet()
    {
        var front = _coordinate.GetFrontPureDir();
        var spline = GetCalculatedSpline(Direction.Up, transform.position + front * _fishingMaxDistance);
        _splineContainer.Spline = spline;
    }

    [ButtonMethod]
    private void DownDirSet()
    {
        var front = _coordinate.GetFrontPureDir();
        var spline = GetCalculatedSpline(Direction.Down, transform.position + front * _fishingMaxDistance);
        _splineContainer.Spline = spline;
    }

    private FishingMinigameController _fishingMinigameController;

    public void BindFishingController(FishingMinigameController fishingMinigameController)
    {
        _fishingMinigameController = fishingMinigameController;
    }

    public void ReleaseFishingController()
    {
        _fishingMinigameController = null;
    }

    public async UniTask<bool> Fishing()
    {
        if (_blackboard.IsInteractionStopped || _blackboard.IsFishingStopped) return false;
        try
        {
            _visual.Animator.SetTrigger("Fishing");
            
            float factor = await _view.Fishing(1f);
            var front = _coordinate.GetFrontPureDir();
            Direction dir;

            factor = Mathf.Max(0.2f, factor);
            
            

            IsFishing = true;

            switch (_move.LastDirection)
            {
                case AnimationActorKey.Direction.Up:
                    dir = Direction.Up;
                    break;
                case AnimationActorKey.Direction.Down:
                    dir = Direction.Down;
                    break;
                case AnimationActorKey.Direction.Left:
                    front.y = 0f;
                    dir = Direction.Left;
                    break;
                case AnimationActorKey.Direction.Right:
                    front.y = 0f;
                    dir = Direction.Right;
                    break;
                case AnimationActorKey.Direction.LeftUp:
                    front.y = 0f;
                    dir = Direction.Left;
                    break;
                case AnimationActorKey.Direction.RightUp:
                    front.y = 0f;
                    dir = Direction.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: 크로니클 용
            if (dir != Direction.Right)
            {
                return false;
            }

            var pos = transform.position + (front * factor * _fishingMaxDistance) + _sideOffset;
            // p1 + ((a * b * c) + d1 * d)

            _visual.Animator.SetBool("IsFishing", true);
            
            Fishing(dir, pos);
            

            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy()
            );

            
            await UniTask.WaitUntil(() => _co is null, PlayerLoopTiming.Update,
                this.GetCancellationTokenOnDestroy());

            AudioManager.Instance.PlayOneShot("SFX", "SFX_Fishing_Colliding_Bait_ToWater");

            var ctx = _fishingMinigameController?.CreateContext();

            if ((ctx?.CanFishingGround(pos) ?? false) is false)
            {
                _visual.Animator.SetBool("IsFishing", false);
                await UnFishing(dir, pos, ctx?.FishTransform);
                ctx?.Release();
                
                _visual.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Right));
                return false;
            }

            _ = ctx.Begin(cts.Token);
            ctx.OnBeginBite += (x) =>
            {
                _fishingStateRenderer.enabled = true;
                
                AudioManager.Instance.PlayOneShot("SFX", "SFX_Fishing_Biting_Bait");
            };
            ctx.OnEndBite += (x) => { _fishingStateRenderer.enabled = false; };

            await UniTask.WaitUntil(() => InputManager.Map.Player.Fishing.triggered, PlayerLoopTiming.Update,
                CancellationTokenSource
                    .CreateLinkedTokenSource(ctx.CancellationToken, this.GetCancellationTokenOnDestroy()).Token);

            ctx.Pause();
            if (ctx.IsTiming)
            {
                _invPresenter.Model.PushItem(ctx.Reward, 1);
                ctx.FishVisible = true;
                _fishingStateRenderer.enabled = false;
                AudioManager.Instance.PlayOneShot("SFX", "SFX_Fishing_Getting_Fish");
            }


            _visual.Animator.SetBool("IsFishing", false);
            await UnFishing(dir, pos, ctx?.FishTransform);

            if (ctx.IsTiming)
            {
                _invPresenter.QuickInvVisible = false;
                
                var inst = DialogueController.Instance;
                inst.Visible = true;
                inst.DialogueText = $"\"{ctx.Reward.ItemName}\"을(를) 낚았다!";

                AudioManager.Instance.PlayOneShot("SFX", "SFX_Fishing_Completion");

                await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                    this.GetCancellationTokenOnDestroy());
                
                
                _invPresenter.QuickInvVisible = true;
                inst.ResetDialogue();
            }
            ctx.Resume();
            
            ctx.Release();


            IsFishing = false;
            _visual.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Right));
            return true;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
        catch (OperationCanceledException)
        {
            _handle.gameObject.SetActive(false);
            _line.gameObject.SetActive(false);
            IsFishing = false;
            _visual.Animator.SetBool("IsFishing", false);
        }

        _visual.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Right));

        return false;
    }

    private async UniTask UnFishing(Direction dir, Vector3 targetPoint, Transform fish = null)
    {
        var spline = GetCalculatedSpline(dir, targetPoint);
        float t = 1f;
        var wPos = transform.position;

        _currenTurningT = false;
        _handle.gameObject.SetActive(true);
        float toTargetPointDis = Vector2.Distance(transform.position, targetPoint);

        var lineSpline = new Spline();

        while (t > 0f)
        {
            var pos = (Vector3)spline.EvaluatePosition(t);
            t -= Time.deltaTime * _speed / toTargetPointDis;

            _handle.position = pos + wPos;

            if (fish)
            {
                fish.position = _handle.position;
            }

            DrawLine(lineSpline, dir, targetPoint, t);

            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

        _handle.gameObject.SetActive(false);
        _line.gameObject.SetActive(false);
    }

    public void Fishing(Direction direction, Vector3 targetPosition)
    {
        if (_co is not null)
        {
            StopCoroutine(_co);
            _co = null;
        }
        
        AudioManager.Instance.PlayOneShot("Player", "Player_Fishing_Swing_Rod");
        
        _handle.position = transform.position;
        StartCoroutine(_co = _Fishing(direction, targetPosition));
    }

    private IEnumerator _Fishing(Direction dir, Vector3 targetPoint)
    {
        var spline = GetCalculatedSpline(dir, targetPoint);
        float t = 0f;
        var wPos = _handle.position;

        _handle.gameObject.SetActive(true);
        _line.gameObject.SetActive(true);
        _currenTurningT = false;

        var lineSpline = new Spline();
        float toTargetPointDis = Vector2.Distance(transform.position, targetPoint);

        while (t <= 1f)
        {
            var pos = (Vector3)spline.EvaluatePosition(t);
            t += Time.deltaTime * _speed / toTargetPointDis;

            _handle.position = pos + wPos;

            DrawLine(lineSpline, dir, targetPoint, t);
            yield return null;
        }

        _co = null;
    }

    private void DrawLine(Spline lineSpline, Direction dir, Vector3 targetPoint, float currentT)
    {
        _line.positionCount = _lineIteraction;

        lineSpline.Clear();
        lineSpline.Add(
            new BezierKnot(transform.position, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)), TangentMode.AutoSmooth);
        lineSpline.Add(
            new BezierKnot(transform.position, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)), TangentMode.AutoSmooth);
        lineSpline.Add(
            new BezierKnot(transform.position, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)),
            dir == Direction.Up || dir == Direction.Down ? TangentMode.AutoSmooth : TangentMode.Broken);

        lineSpline.SetKnot(0,
            new BezierKnot(transform.position, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)));

        Vector3 toHandleDir = (_handle.position - transform.position);
        float t = Mathf.Clamp(Mathf.Abs(toHandleDir.x) - _beginLineLength, 0, _maxLineLength - _beginLineLength) /
                  (_maxLineLength - _beginLineLength);

        if (currentT >= _turningT && (dir == Direction.Left || dir == Direction.Right))
        {
            _currenTurningT = true;
        }

        float toHandleDis = Mathf.Lerp(0f, _lineHeightFactor * (_currenTurningT ? -1f : 1f), t);

        float handleToTargetDis = Vector3.Distance(targetPoint, _handle.position);

        if (handleToTargetDis <= _maxHandleInTagent)
        {
            handleToTargetDis /= _maxHandleInTagent;
        }
        else
        {
            handleToTargetDis = 1f;
        }

        Vector3 middlePos =
            Vector3.Lerp(transform.position, _handle.position, 0.5f) + Vector3.down * toHandleDis;

        lineSpline.SetKnot(1,
            new BezierKnot(middlePos, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)));

        lineSpline.SetKnot(2,
            new BezierKnot(_handle.position, new float3(2.19f, 3.26f, 0f) * handleToTargetDis, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)));

        for (int i = 0; i < _lineIteraction; i++)
        {
            var linePos = lineSpline.EvaluatePosition(i / (float)(_lineIteraction - 1));

            _line.SetPosition(i, linePos);
        }
    }


    private Spline GetCalculatedSplineVertical(Direction dir, Vector3 targetPoint)
    {
        Spline spline = new Spline();

        Vector3 startPoint = Vector3.zero;
        Vector3 dirV = Vector3.one;
        float factor = 0f;

        targetPoint = transform.worldToLocalMatrix.MultiplyPoint(targetPoint);

        switch (dir)
        {
            case Direction.Up:
                dirV = Vector3.down;
                factor = _verticalUpFactor;
                break;
            case Direction.Down:
                dirV = Vector3.up;
                factor = _verticalDownFactor;
                break;
            default:
                Debug.Assert(false);
                break;
        }

        factor = Vector2.Distance(targetPoint, startPoint) * factor;
        dirV = dirV.normalized * factor;


        spline.Add(
            new BezierKnot(startPoint, float3.zero, float3.zero, quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)),
            TangentMode.Continuous);
        spline.Add(
            new BezierKnot(startPoint + dirV, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)), TangentMode.Continuous);
        spline.Add(
            new BezierKnot(targetPoint, float3.zero, float3.zero, quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)),
            TangentMode.Continuous);

        return spline;
    }

    private Spline GetCalculatedSplineHorizontal(Direction dir, Vector3 targetPoint)
    {
        Spline spline = new Spline();

        Vector3 startPoint = Vector3.zero;
        Vector3 dirV = Vector3.one;

        targetPoint = transform.worldToLocalMatrix.MultiplyPoint(targetPoint);


        switch (dir)
        {
            case Direction.Left:
                dirV = Quaternion.AngleAxis(_horizontalDirAngle, Vector3.forward) * Vector3.right;
                dirV.x *= -1f;
                break;
            case Direction.Right:
                dirV = Quaternion.AngleAxis(_horizontalDirAngle, Vector3.forward) * Vector3.right;
                break;
            default:
                Debug.Assert(false);
                break;
        }

        dirV = dirV.normalized;

        spline.Add(
            new BezierKnot(startPoint, float3.zero, float3.zero, quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)),
            TangentMode.Continuous);


        /*
        OB: ToTarget
        OB = |OA`| * cos * OB/|OB|
        OB * 1 / cos = |OA`| * OB/|OB|
        |(OB * 1/cos)| = |OA`|
         */
        var toTarget = (Vector3)((Vector3)targetPoint - startPoint);
        Vector3 projV = toTarget
                        * (1f / (Vector3.Dot(dirV.normalized, toTarget.normalized)))
            ;

        float projVMag = projV.magnitude;
        dirV = dirV.normalized * projVMag;
        dirV.x = targetPoint.x;
        dirV.y = Mathf.Min(dirV.y, _sideMaxY);

        spline.Add(
            new BezierKnot(startPoint + Vector3.one * 0.000001f, float3.zero, dirV,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)), TangentMode.Broken);


        spline.Add(
            new BezierKnot((Vector3)targetPoint, float3.zero, float3.zero,
                quaternion.AxisAngle(new float3(0f, 0f, 1f), 0f)), TangentMode.Broken);

        return spline;
    }

    public Spline GetCalculatedSpline(Direction dir, Vector2 targetPoint)
    {
        if (dir == Direction.Up || dir == Direction.Down)
        {
            return GetCalculatedSplineVertical(dir, targetPoint);
        }
        else
        {
            return GetCalculatedSplineHorizontal(dir, targetPoint);
        }
    }
}