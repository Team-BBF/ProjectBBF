using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

[RequireComponent(typeof(SceneCapture), typeof(DetailLoader))]
public class DetailGenerator : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private string _captureSaveFilePath;
    [SerializeField] private string _detailToLoadFilePath;
    [SerializeField] private string _uniqueFileName;

    [SerializeField] private Vector2Int _resoultion;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private Vector2Int _iteration;

    [SerializeField] private int _ppu;
    [SerializeField] private int _captureDelay = 100;
    [field: SerializeField, SortingLayer] private int _sortingLayer;
    [SerializeField] private int _order;

    [SerializeField] private Transform _content;
    
    [SerializeField] private bool _onVisualizer;

    private float Unit => SceneCaptureUtility.CalculateUnit(_resoultion.x, _ppu);
    private float HalfUnit => Unit * 0.5f;

    public void ApplyData()
    {
        Capture.Camera = _camera;
        Capture.FilePath = _captureSaveFilePath;
        Capture.UniqueFileName = _uniqueFileName;
        Capture.Resoultion = _resoultion;
        Capture.Offset = _offset;
        Capture.Iteration = _iteration;
        Capture.PPU = _ppu;
        Capture.CaptureDelay = _captureDelay;

        Loader.DetailsPath = _detailToLoadFilePath;
        Loader.MapName = _uniqueFileName;
        Loader.Resolution = _resoultion;
        Loader.PPU = _ppu;
        Loader.Offset = _offset;
        Loader.Iteration = _iteration;
        Loader.SortingLayer = _sortingLayer;
        Loader.Order = _order;
        Loader.Content = _content;
    }

    [field: SerializeField, HideInInspector]
    private SceneCapture _capture;

    [field: SerializeField, HideInInspector]
    private DetailLoader _loader;

    public SceneCapture Capture
    {
        get
        {
            if (_capture == false)
            {
                _capture = gameObject.GetComponent<SceneCapture>();
            }

            return _capture;
        }
    }

    public DetailLoader Loader
    {
        get
        {
            if (_loader == false)
            {
                _loader = gameObject.GetComponent<DetailLoader>();
            }

            return _loader;
        }
    }

    [ButtonMethod]
    private void BeginCapture()
    {
        Capture.OnPlay = true;
        ApplyData();
        Capture.Capture();
    }

    [ButtonMethod]
    private void ApplyDetails()
    {
        ApplyData();
        Loader.LoadDetails();
    }


    private Color[,] _colors;
    private void OnValidate()
    {
        _camera.orthographicSize = HalfUnit;
        SceneCaptureUtility.Redraw(Unit, _iteration, out _colors);
    }

    private void OnDrawGizmosSelected()
    {
        if (_onVisualizer == false) return;
        
        SceneCaptureUtility.DrawGizmos(_offset, Unit, _iteration, _colors);
    }
}
#else

public class DetailGenerator : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }

}
#endif