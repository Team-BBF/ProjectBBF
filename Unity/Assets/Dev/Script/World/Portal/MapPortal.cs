using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class MapPortal : MonoBehaviour
{
    [field: SerializeField, HideInInspector] private string scene;
    [SerializeField] private string _portalKey;
    [field: SerializeField] private string _targetPortalKey;
    [SerializeField] private Vector2 _alivePosition;

    public string PortalKey => _portalKey;

    public string TargetPortalKey => _targetPortalKey;

    public Vector2 AlivePosition => _alivePosition;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset _sceneAsset;

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (_sceneAsset == false) return;
        
        scene = _sceneAsset.name;
    }
    
#endif
    private void Awake()
    {
         MapPortalManager.Instance.TryAdd(_portalKey, AlivePosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(scene)) return;
        
        if (other.TryGetComponent(out PlayerController c))
        {
            TimeManager.Instance.Pause();
            
            c.StateHandler.TranslateState("DoNothing");
            _ = MapPortalManager.Instance.Move(scene, _targetPortalKey, c.transform)
                .ContinueWith(_ =>
                {
                    TimeManager.Instance.Resume();
                    c.StateHandler.TranslateState("EndOfDoNothing");
                });
        }
    }
}
