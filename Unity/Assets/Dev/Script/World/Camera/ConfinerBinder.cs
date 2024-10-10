using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfinerBinder : MonoBehaviour
{
    [field: SerializeField, AutoProperty] private CinemachineConfiner2D _cinemachine;
    [field: SerializeField, AutoProperty] private Camera _camera;

    private void Awake()
    {
        SceneLoader.Instance.WorldPostLoaded += OnLoaded;
        OnLoaded("");
    }

    private void OnDestroy()
    {
        if (SceneLoader.Instance == false) return;
        
        SceneLoader.Instance.WorldPostLoaded -= OnLoaded;
    }

    private void OnLoaded(string worldSceneName)
    {
        var obj = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(x => x.CompareTag("Confiner"));
        
        if (obj is null || _cinemachine == false) return;
        if (obj.TryGetComponent(out PolygonCollider2D col))
        {
            _cinemachine.InvalidateCache();
            _cinemachine.m_BoundingShape2D = col;
        }
    }
}
