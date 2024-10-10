using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameQuitView : MonoBehaviour
{
    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    public void Init()
    {
        
    }
}
