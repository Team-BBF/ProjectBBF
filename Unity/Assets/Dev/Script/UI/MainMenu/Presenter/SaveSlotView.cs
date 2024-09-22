using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProjectBBF.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text _saveName;
    [SerializeField] private TMP_Text _saveDescription;
    [SerializeField] private Button _btn;
    
    public Metadata CurrentMetaData { get; private set; }

    public string SaveName
    {
        get=> _saveName.text;
        set => _saveName.text = value;
    }

    public string SaveDescription => _saveDescription.text;

    private void SetDescription(string charName, int day)
    {
        _saveDescription.text = $"{charName}, {day:D3}";
    }

    public SaveSlotView Clone(Metadata metadata)
    {
        var obj = Instantiate(this);
        
        obj.SaveName = metadata.SaveFileName;
        obj.SetDescription(metadata.PlayerName, metadata.Day);
        obj.CurrentMetaData = metadata;

        return obj;
    }

    private void Awake()
    {
        _btn.onClick.AddListener(() =>
        {
            if (CurrentMetaData != null)
            {
                PersistenceManager.Instance.CurrentMetadata = CurrentMetaData;
                PersistenceManager.Instance.LoadGameDataCurrentFileName();
                
                SceneLoader.Instance.WorkDirectorAsync(false, "BlackAlpha")
                    .ContinueWith(_ => SceneLoader.Instance.LoadWorldAsync("World_DaffodilLake_Arakar"))
                    .ContinueWith(_ => SceneLoader.Instance.LoadImmutableScenesAsync())
                    .ContinueWith(_ => SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlpha"))
                    .Forget();
            }
        });
    }
}
