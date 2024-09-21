using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPannelController : MonoBehaviour
{
    [SerializeField] private PlayerMainInventoryView _invView;
    [SerializeField] private PlayerSettingView _settingView;
    [SerializeField] private PlayerGameQuitView _quitView;

    public enum ViewType : int
    {
        Close,
        Inv,
        Setting,
        Quit,
    }

    private ViewType _viewState;
    public ViewType ViewState
    {
        get => _viewState;
        set
        {
            _viewState = value;

            _settingView.Visible = false;
            _invView.Visible = false;
            _quitView.Visible = false;
            
            
            gameObject.SetActive(_viewState != ViewType.Close);
            
            switch (_viewState)
            {
                case ViewType.Close:
                    break;
                case ViewType.Inv:
                    _invView.Visible = true;
                    break;
                case ViewType.Setting:
                    _settingView.Visible = true;
                    break;
                case ViewType.Quit:
                    _quitView.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void SetViewState(int type)
    {
        ViewState = (ViewType)type;
    }

    private void Awake()
    {
        _invView.Init();
        _settingView.Init();
        _quitView.Init();
        
        ViewState = ViewType.Close;
    }
}