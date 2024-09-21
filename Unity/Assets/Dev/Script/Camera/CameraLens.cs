using Cinemachine;
using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLens : MonoBehaviour
{
    [field: SerializeField, MustBeAssigned, InitializationField, AutoProperty]
    private CinemachineVirtualCamera _camera;

    [SerializeField] private float _ppu;
    [SerializeField] private float _size;

    private void Awake()
    {
        ScreenManager.Instance.OnChangedResolution += LensUpdate;
        LensUpdate(ScreenManager.Instance.CurrentResolution);
    }

    public void LensUpdate(Vector2Int resolution)
    {
        var lens = _camera.m_Lens;

        float height = resolution.y;

        if (height <= 1080)
        {
            height = 1080f;
        }
        
        lens.OrthographicSize = (height / _ppu) * 0.5f * _size;
        _camera.m_Lens = lens;

        if (_camera.TryGetComponent<CinemachineConfiner2D>(out var confiner))
        {
            confiner.InvalidateCache();
        }
    }
}
