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
        
        Metadata[] saves = PersistenceManager.GetAllSaveFileMetadata();

        foreach (Metadata save in saves)
        {
            var slot = _saveSlotPrototype.Clone(save);
            slot.transform.SetParent(_content);
            slot.gameObject.SetActive(true);
        }
        
    }

    private void OnEnable()
    {
        LoadSaveFile();
    }
    
}
