using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class QuestIndicatorUI : ActorComponent
{
    [SerializeField] private Color _fadeUiColor;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private ESOQuest _esoQuest;
    [SerializeField] private GameObject _renderer;
    [SerializeField] private Transform _pivot;

    [SerializeField] private List<QuestData> _targetQuestList;

    private Camera _cachedCamera;
    private Canvas _canvas;
    private bool _isWorldCanvas;
    private Tweener _scaleTweener;

    private InputAction _keyAction;
    private bool _keyFlag;

    private bool _isRaised;

    private HashSet<string> _createdQuestHashset = new HashSet<string>();
    
    private Camera CachedCamera
    {
        get
        {
            if (_cachedCamera == false)
            {
                _cachedCamera = Camera.main;
            }

            return _cachedCamera;
        }
    }

    public bool Visible
    {
        get => gameObject.activeSelf && _isRaised;
        set => gameObject.SetActive(value && _isRaised);
    }
    
    public void Init()
    {
        if (_esoQuest == false) return;

        _esoQuest.OnEventRaised += OnEventRaised;
        gameObject.SetActive(false);
        foreach (QuestData questData in _targetQuestList)
        {
            if (questData)
            {
                if (QuestManager.Instance && QuestManager.Instance.GetQuestState(questData.QuestKey) == QuestType.Create)
                {
                    _isRaised = true;
                    Visible = true;
                    _createdQuestHashset.Add(questData.QuestKey);
                }
            }
        }
    }

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        
        RectTransform boundTransform = _renderer.transform as RectTransform;
        _savedScale = boundTransform.localScale;

        _keyAction = InputManager.Map.UI.ForcusQuestMarker;
    }

    private void OnKeyDown(InputAction.CallbackContext obj)
    {
        _keyFlag = true;
    }
    private void OnKeyUp(InputAction.CallbackContext obj)
    {
        _keyFlag = true;
    }

    private void OnDestroy()
    {
        _scaleTweener?.Kill();
        
        if (_esoQuest == false) return;
        
        _esoQuest.OnEventRaised -= OnEventRaised;
    }

    private void OnEventRaised(QuestEvent obj)
    {
        if (this == false)
        {
            _esoQuest.OnEventRaised -= OnEventRaised;
            return;
        }
        
        if (_targetQuestList.FirstOrDefault(x => x.QuestKey == obj.QuestKey) == false) return;
        
        if (obj.Type is QuestType.Create)
        {
            _isRaised = true;
            gameObject.SetActive(true);
            _ = _createdQuestHashset.Add(obj.QuestKey);
        }
        else if(obj.Type is QuestType.Cancele or QuestType.Complete)
        {
            _ = _createdQuestHashset.Remove(obj.QuestKey);
            if (_createdQuestHashset.Any()) return;
            
            _isRaised = false;
            gameObject.SetActive(false);
        }
        
    }

    private Vector2 _savedScale;
    private void UpdatePosition(bool isWorldCanvas)
    {
        RectTransform boundTransform = _renderer.transform as RectTransform;
        Camera camera = CachedCamera;
        if (camera == false) return;
        
        Debug.Assert(boundTransform);
        Debug.Assert(_renderer);
        Debug.Assert(_pivot);

        float scaleFactor = isWorldCanvas ? (0.5f * camera.scaledPixelHeight / camera.orthographicSize) : 1f;
        scaleFactor *= _canvas.scaleFactor;

        Vector2 cameraScaledSize = new Vector2(camera.scaledPixelWidth, camera.scaledPixelHeight);
        Vector2 boundSize = boundTransform.rect.size * boundTransform.lossyScale * scaleFactor;
        Vector2 pos = camera.WorldToScreenPoint(_pivot.position);
        pos = ScreenUtils.ScreenToOrthogonal(pos, cameraScaledSize);

        pos = ScreenUtils.ToValidPosition(
            boundSize,
            cameraScaledSize,
            pos
        );
        
        if (QuestManager.Instance)
        {
            if (_keyAction.ReadValue<float>() > 0f && _keyFlag is false)
            {
                foreach (QuestIndicatorObstacleUI obstacle in QuestManager.Instance.IndicatorObstacleList)
                {
                    obstacle.DoFade(0.2f, 0.2f);
                    _scaleTweener?.Kill();
                    _scaleTweener = boundTransform.DOScale(_savedScale * 1.5f, 0.3f).SetId(this).SetLoops(-1, LoopType.Yoyo);
                }
                _keyFlag = true;
            }
            else if(_keyAction.ReadValue<float>() <= 0f && _keyFlag)
            {
                foreach (QuestIndicatorObstacleUI obstacle in QuestManager.Instance.IndicatorObstacleList)
                {
                    obstacle.DoFade(1f, 0.2f);
                    _scaleTweener?.Kill();
                    _scaleTweener = boundTransform.DOScale(_savedScale, 0.3f).SetId(this);
                }
                _keyFlag = false;
            }
        }
        
        pos = ScreenUtils.OrthogonalToScreen(pos, cameraScaledSize);

        if (isWorldCanvas)
        {
            pos = camera.ScreenToWorldPoint(pos);
        }



        boundTransform.position = Vector2.Lerp(boundTransform.position, pos, Time.deltaTime / _duration);
    }

    private void LateUpdate()
    {
        UpdatePosition(_canvas.renderMode == RenderMode.WorldSpace);
    }
}
