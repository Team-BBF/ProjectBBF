using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;

public class HomeBed : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GotoNextDay(other.GetComponent<PlayerController>());
        }
    }

    private void GotoNextDay(PlayerController controller)
    {
        EventController.Instance.SignalEvent(new FlowNextDay());
    }
}