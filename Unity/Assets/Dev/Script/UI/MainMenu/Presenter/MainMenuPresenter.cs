using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuPresenter : MonoBehaviour
{
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private GameObject _selectSavePanel;
    [SerializeField] private GameObject _newSavePanel;

    [SerializeField] private SettingView _settingView;
    [SerializeField] private NewSaveView _newSaveView;

    [SerializeField] private GameObject _mainMenuFrame;

    private void Awake()
    {
        _settingView.Init();
        _settingView.Visible = true;

        SetActivePanels(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void CreateNewSave()
    {
        _newSaveView.GetTexts(out string charName, out string worldName);

        PersistenceManager.Instance.CurrentMetadata = new Metadata()
        {
            SaveFileName = worldName,
            PlayerName = charName
        };
        
        PersistenceManager.Instance.NewGameDataCurrentFileName();
        
        SceneLoader.Instance.WorkDirectorAsync(false, "BlackAlpha")
            .ContinueWith(_ => SceneLoader.Instance.LoadWorldAsync("World_Opening"))
            .ContinueWith(_ => SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlpha"))
            .Forget();
    }
    public void SetActivePanels(bool active)
    {
        _settingPanel.SetActive(active);
        _selectSavePanel.SetActive(active);
        _newSavePanel.SetActive(active);
    }

    public void ToggleSetting()
    {
        bool active = _settingPanel.activeSelf;

        SetActivePanels(false);

        _mainMenuFrame.SetActive(active);
        _settingPanel.SetActive(!active);
    }

    public void ToggleSaveSlot()
    {
        bool active = _selectSavePanel.activeSelf;

        SetActivePanels(false);

        _mainMenuFrame.SetActive(active);
        _selectSavePanel.SetActive(!active);
    }
    public void ToggleNewSaveSlot()
    {
        bool active = _newSavePanel.activeSelf;

        SetActivePanels(false);

        _mainMenuFrame.SetActive(active);
        _newSavePanel.SetActive(!active);
    }
}