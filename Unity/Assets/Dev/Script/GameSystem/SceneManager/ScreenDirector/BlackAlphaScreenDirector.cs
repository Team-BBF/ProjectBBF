using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BlackAlphaScreenDirector : ScreenDirector
{
    [SerializeField] private string _key;
    [SerializeField] private Image _panel;
    [SerializeField] private float _fadeoutDuration;
    [SerializeField] private float _fadeinDuration;
    [SerializeField] private float _waitDuration;

    public override string Key => _key;

    public override bool Enabled
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    public override UniTask Fadein()
    {
        _panel.color = Color.black;
        
        var c = Color.black;
        c.a = 0f;
        return _panel
            .DOColor(c, _fadeinDuration).SetEase(Ease.Linear)
            .SetDelay(_waitDuration)
            .AsyncWaitForCompletion()
            .AsUniTask()
            .WithCancellation(GlobalCancelation.PlayMode)
            ;
    }

    public override UniTask Fadeout()
    {
        var c = Color.black;
        c.a = 0f;
        _panel.color = c;
        
        c.a = 1f;
        return _panel
                .DOColor(c, _fadeoutDuration).SetEase(Ease.Linear)
                .AsyncWaitForCompletion()
                .AsUniTask()
                .WithCancellation(GlobalCancelation.PlayMode)
            ;
    }
}
