using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private GameObject _selectSavePanel;

    [SerializeField] private PlayerSettingView _settingView;

    [SerializeField] private GameObject _mainMenuFrame;

    private void Awake()
    {
        _settingView.Init();
        _settingView.Visible = true;

        _settingPanel.SetActive(false);
        _selectSavePanel.SetActive(false);
    }

    public void GotoGameWorld()
    {
        SceneLoader.Instance.WorkDirectorAsync(false, "BlackAlpha")
            .ContinueWith(_ => SceneLoader.Instance.LoadWorldAsync("World_DaffodilLake_Arakar"))
            .ContinueWith(_ => SceneLoader.Instance.LoadImmutableScenesAsync())
            .ContinueWith(_ => SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlpha"))
            .Forget();
    }

    public void Quit()
    {
        Application.Quit();
    }
    
    public void SetActivePanels(bool active)
    {
        _settingPanel.SetActive(active);
        _selectSavePanel.SetActive(active);
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
}