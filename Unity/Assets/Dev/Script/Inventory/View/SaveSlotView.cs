using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text _saveName;
    [SerializeField] private TMP_Text _saveDescription;

    public string SaveName
    {
        get=> _saveName.text;
        set => _saveName.text = value;
    }

    public string SaveDescription => _saveDescription.text;

    public void SetDescription(string charName, int day)
    {
        _saveDescription.text = $"{charName}, {day:D3}";
    }

    public SaveSlotView Clone(string saveName, string charName, int day)
    {
        var obj = Instantiate(this);
        
        obj.SaveName = saveName;
        obj.SetDescription(charName, day);

        return obj;
    }
}
