using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(menuName = "ProjectBBF/Project/ImmutableSceneTable", fileName = "ImmutableSceneTable")]
public class ImmutableSceneTable : ScriptableObject
{
    [field: SerializeField, HideInInspector]
    private List<string> _scenes;
    
#if UNITY_EDITOR
    [SerializeField] public List<SceneAsset> SceneAssets;

    private void OnValidate()
    {
        _scenes = new List<string>();
        SceneAssets?.ForEach(x=>_scenes.Add(x.name));
    }
#endif

    public IReadOnlyList<string> Scenes => _scenes;
}
