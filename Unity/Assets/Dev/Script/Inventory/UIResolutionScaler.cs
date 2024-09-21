using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResolutionScaler : MonoBehaviour
{
    [SerializeField] private Vector2Int _standardResolution = new Vector2Int(1920, 1080);

    [SerializeField] private int _upperCase;
    [SerializeField] private int _lowerCase;

    // Scale With Screen Size
    [SerializeField] private Vector2Int _upperReferenceResolution = new Vector2Int(1920, 1080);
    [SerializeField] private CanvasScaler.ScreenMatchMode _upperScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;


    // Constant Pixel size
    [SerializeField] private float _upperScaleFactor = 1f;

    // Constant Physics size
    [SerializeField] private CanvasScaler.Unit _upperPhysicalUnit = CanvasScaler.Unit.Points;
    [SerializeField] private int _upperFallbackScreenDPI = 96;
    [SerializeField] private int _upperDefaultSpriteDPI = 96;

    // common
    [SerializeField] private int _upperReferencePixelsPerUnit = 100;


    // Scale With Screen Size
    [SerializeField] private Vector2Int _lowerReferenceResolution = new Vector2Int(1920, 1080);
    [SerializeField] private CanvasScaler.ScreenMatchMode _lowerScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;


    // Constant Pixel size
    [SerializeField] private float _lowerScaleFactor = 1f;

    // Constant Physics size
    [SerializeField] private CanvasScaler.Unit _lowerPhysicalUnit = CanvasScaler.Unit.Points;
    [SerializeField] private int _lowerFallbackScreenDPI = 96;
    [SerializeField] private int _lowerDefaultSpriteDPI = 96;

    // common
    [SerializeField] private int _lowerReferencePixelsPerUnit = 100;

    private CanvasScaler _scaler;
    private ScreenManager _manager;

    private void Awake()
    {
        _manager = ScreenManager.Instance;

        if (TryGetComponent<CanvasScaler>(out var scaler))
        {
            _scaler = scaler;
            _manager.OnChangedResolution += OnChangedResolution;
            OnChangedResolution(_manager.CurrentResolution);
        }
    }

    private void OnChangedResolution(Vector2Int resolution)
    {
        if (_scaler == false) return;
        if (_manager == false) return;

        if (resolution.y < _standardResolution.y || resolution.x < _standardResolution.x)
        {
            SetScalePropertiesLower(_lowerCase);
        }
        else
        {
            SetScalePropertiesUpper(_upperCase);
        }
    }

    private void SetScalePropertiesUpper(int index)
    {
        if (index == 0)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = _upperReferenceResolution;
            _scaler.screenMatchMode = _upperScreenMatchMode;
        }
        else if (index == 1)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _scaler.scaleFactor = _upperScaleFactor;
        }
        else if (index == 2)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
            _scaler.physicalUnit = _upperPhysicalUnit;
            _scaler.fallbackScreenDPI = _upperFallbackScreenDPI;
            _scaler.defaultSpriteDPI = _upperDefaultSpriteDPI;
        }

        _scaler.referencePixelsPerUnit = _upperReferencePixelsPerUnit;
    }
    private void SetScalePropertiesLower(int index)
    {
        if (index == 0)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = _lowerReferenceResolution;
            _scaler.screenMatchMode = _lowerScreenMatchMode;
        }
        else if (index == 1)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _scaler.scaleFactor = _lowerScaleFactor;
        }
        else if (index == 2)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
            _scaler.physicalUnit = _lowerPhysicalUnit;
            _scaler.fallbackScreenDPI = _lowerFallbackScreenDPI;
            _scaler.defaultSpriteDPI = _lowerDefaultSpriteDPI;
        }

        _scaler.referencePixelsPerUnit = _lowerReferencePixelsPerUnit;
    }
}