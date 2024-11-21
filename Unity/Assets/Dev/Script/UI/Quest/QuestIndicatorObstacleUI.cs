using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MyBox; // DOTween 네임스페이스 추가

[RequireComponent(typeof(RectTransform))]
public class QuestIndicatorObstacleUI : MonoBehaviour
{
    [SerializeField] private bool _ignore;
    
    private Tween _currentTextTween;
    private Tween _currentImageTween;

    public bool IsFadeAnimating { get; private set; }

    [SerializeField] private List<Graphic> _renderComList;
    private float _currentAlpha = 1f;

    [ButtonMethod]
    private void Bind()
    {
        Queue<Transform> queue = new(2);
        queue.Enqueue(transform);
        _renderComList = new(2);

        while (queue.Any())
        {
            var targetTransform = queue.Dequeue();
            if (targetTransform != transform && targetTransform.TryGetComponent(out QuestIndicatorObstacleUI _))
            {
                continue;
            }
            
            var comArr = targetTransform.GetComponents<Graphic>();
            _renderComList.AddRange(comArr);

            foreach (Transform childTransform in targetTransform)
            {
                queue.Enqueue(childTransform);
            }
        }
    }

    private void OnValidate()
    {
        Bind();
    }

    public void DoFade(float fadeAlpha, float duration)
    {
        if (_ignore) return;
        
        StopAllCoroutines();
        StartCoroutine(CoDoFade(fadeAlpha, duration));
    }

    private IEnumerator CoDoFade(float endAlpha, float duration)
    {
        float t = 0f;
        IsFadeAnimating = true;
        while (true)
        {
            foreach (MaskableGraphic com in _renderComList)
            {
                if (com)
                {
                    com.SetAlpha(_currentAlpha);
                }
            }

            if (t >= 1f)
            {
                IsFadeAnimating = false;
                break;
            }

            t += Time.deltaTime / duration;
            _currentAlpha = Mathf.Lerp(_currentAlpha, endAlpha, t);

            yield return null;
        }
        
        IsFadeAnimating = false;
    }

    private void OnEnable()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.IndicatorObstacleList.Add(this);
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.IndicatorObstacleList.Remove(this);
        }
    }
}
