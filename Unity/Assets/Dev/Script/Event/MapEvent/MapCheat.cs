


using System;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapCheat : MonoBehaviour
{
    [SerializeField] private Transform _point;
    [SerializeField] private PolygonCollider2D _collider;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            if (_point && GameObjectStorage.Instance)
            {
                if (GameObjectStorage.Instance.TryGetPlayerController(out var pc))
                {
                    if (Camera.main.TryGetComponent(out ConfinerBinder binder))
                    {
                        pc.transform.position = _point.position;
                        binder.Confiner = _collider;
                    }
                }
            }
            
        }
    }
}