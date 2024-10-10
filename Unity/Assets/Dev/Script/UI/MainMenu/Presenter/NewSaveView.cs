using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewSaveView : MonoBehaviour
{
    [SerializeField] private TMP_InputField _charName;
    [SerializeField] private TMP_InputField _worldName;

    public void GetTexts(out string charName, out string worldName)
    {
        charName = _charName.text;
        worldName = _worldName.text;
    }

    private void Awake()
    {
        _charName.text = "";
        _worldName.text = "";
    }
}
