using System;
using UnityEngine;


public class SettingAudioModel
{
    public event Action<(string groupKey, float value)> OnChangedVolume;
    
    public SettingAudioModel()
    {
        if (AudioManager.Instance == false) return;

        AudioManager.Instance.OnChangedVolume += _OnChangedVolume;
    }


    public void Release()
    {
        if (AudioManager.Instance == false) return;
        
        AudioManager.Instance.OnChangedVolume -= _OnChangedVolume;
    }

    private void _OnChangedVolume(string arg1, float arg2)
    {
        OnChangedVolume?.Invoke((arg1, arg2));
    }

    public float GetVolume(string groupKey)
    {
        if (AudioManager.Instance == false) return 0f;
        return AudioManager.Instance.GetVolume(groupKey);
    }

    public void SetVolume(string groupKey, float value)
    {
        if (AudioManager.Instance == false) return;
        AudioManager.Instance.SetVolume(groupKey, value);
    }

    public void ApplySetting()
    {
        if (AudioManager.Instance == false) return;
        AudioManager.Instance.SaveSetting();
    }
}