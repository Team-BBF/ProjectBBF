using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

[RequireComponent(typeof(MapCutSceneEventReceiver))]
public class MapCollisionTrigger : MonoBehaviour
{
    [field: SerializeField, AutoProperty]
    private MapCutSceneEventReceiver _receiver;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _receiver.Play();
        }
    }
}
