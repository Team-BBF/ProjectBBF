using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfinerTag : MonoBehaviour
{
    private void Awake()
    {
        GameObjectStorage.Instance.AddGameObject(gameObject);
    }

    private void OnDestroy()
    {
        if(GameObjectStorage.Instance == false)return;
        
        GameObjectStorage.Instance.RemoveGameObject(gameObject);
    }
}
