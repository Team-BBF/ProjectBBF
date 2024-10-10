using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[System.Serializable]
public class SceneName
{
    #if UNITY_EDITOR
    [SerializeField] private SceneAsset _sceneAsset;
    #endif
    
    [SerializeField] private string _sceneName;

    public string Scene => _sceneName;
    
    public static implicit operator string(SceneName name)
    {
        if(name is null) return string.Empty;

        return name._sceneName;
    }
}