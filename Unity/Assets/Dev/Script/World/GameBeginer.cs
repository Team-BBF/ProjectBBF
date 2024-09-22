using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Event;
using UnityEngine;

public class GameBeginer : MonoBehaviour
{
    private void Start()
    {
        TimeManager.Instance.Begin();
    }
}
