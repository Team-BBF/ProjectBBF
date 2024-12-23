using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.SceneManagement;

[Singleton(ESingletonType.Global, 100)]
public class GameObjectStorage : MonoBehaviourSingleton<GameObjectStorage>
{
    private List<GameObject> _storedObjects = new(10);

    public IReadOnlyList<GameObject> StoredObjects => _storedObjects;

    private readonly List<string> _sceneTemp = new(50);
    private readonly Queue<Transform> _objTemp = new(50);
    
    private PlayerController _cachedPlayer;

    
    public bool TryGetPlayerController(out PlayerController pc)
    {
        if (_cachedPlayer)
        {
            pc = _cachedPlayer;
            return true;
        }
        
        foreach (GameObject obj in StoredObjects)
        {
            if (obj && obj.CompareTag("Player") && obj.TryGetComponent(out PlayerController player))
            {
                _cachedPlayer = player;
                pc = player;
                return true;
            }
        }

        pc = null;
        return false;
    }
    
    public override void PostInitialize()
    {
    }

    public override void PostRelease()
    {
        _cachedPlayer = null;
    }

    public void AddGameObject(GameObject obj)
    {
        if (_storedObjects.Contains(obj)) return;
        _storedObjects.Add(obj);
    }

    public void RemoveGameObject(GameObject obj)
    {
        _storedObjects.Remove(obj);
    }


    private void CheckChangeScenes()
    {
        var inst = SceneLoader.Instance;


        _sceneTemp.Clear();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            _sceneTemp.Add(SceneManager.GetSceneAt(i).name);
        }
    }

    public void ForEach(Func<GameObject, bool> callback)
    {
        CheckChangeScenes();
        
        foreach (var sceneName in _sceneTemp)
        {
            var objs = SceneManager.GetSceneByName(sceneName).GetRootGameObjects();

            foreach (GameObject obj in objs)
            {
                if (ObjectLoop(obj, callback) is false)
                {
                    return;
                }
            }
        }
    }

    private bool ObjectLoop(GameObject obj, Func<GameObject, bool> callback)
    {
        if (callback(obj) is false)
        {
            return false;
        }
        
        _objTemp.Clear();
        _objTemp.Enqueue(obj.transform);

        while (_objTemp.Any())
        {
            int length = _objTemp.Count;
            for (int i = 0; i < length; i++)
            {
                var temp = _objTemp.Dequeue();
            
                for (int j = 0; j < temp.childCount; j++)
                {
                    if (callback(temp.gameObject) is false)
                    {
                        _objTemp.Clear();
                        return false;
                    }
                
                    _objTemp.Enqueue(temp.GetChild(j));
                }
            }
        }

        return true;
    }
}
