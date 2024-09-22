using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudController : MonoBehaviour
{
    public bool Visible
    {
        get => gameObject.activeSelf;
        set=> gameObject.SetActive(value);
    }
}
