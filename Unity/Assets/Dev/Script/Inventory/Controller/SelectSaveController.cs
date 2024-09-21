using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

public class SelectSaveController : MonoBehaviour
{
    [SerializeField] private SaveSlotView _saveSlotPrototype;

    [SerializeField] private Transform _content;
    
    public void LoadSaveFile()
    {
        for (int i = 0; i < _content.childCount; i++)
        {
            Destroy(_content.GetChild(i).gameObject);
        }
        
        string[] saves = PersistenceManager.GetAllSaveDataName();

        foreach (string save in saves)
        {
            var saveName = save.Split(".")[0];
            var slot = _saveSlotPrototype.Clone(saveName, "Default", 1);
            slot.transform.SetParent(_content);
            slot.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        LoadSaveFile();
    }
    
}
