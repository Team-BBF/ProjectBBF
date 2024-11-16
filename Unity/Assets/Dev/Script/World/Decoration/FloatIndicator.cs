using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class FloatIndicator : MonoBehaviour
{
    [SerializeField] private Ease _ease = Ease.Linear;
    [SerializeField] private float _duration;
    [SerializeField] private float _startDelay;
    [SerializeField] private float _rangeY;

    private Tweener _tweener;
    
    private void OnEnable()
    {
        _tweener?.CompletedLoops();
        _tweener?.Rewind();
        _tweener = transform
            .DOLocalMoveY(transform.localPosition.y + _rangeY, _duration)
            .SetDelay(_startDelay)
            .SetEase(_ease)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        _tweener?.CompletedLoops();
        _tweener?.Rewind();
        _tweener = null;
    }

    public void OnFade(float f)
    {
        if (Mathf.Approximately(f, 0f))
        {
            OnDisable();
        }
        else if(_tweener is null)
        {
            OnEnable();
        }
    }
}
