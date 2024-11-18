


using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MyBox;
using TMPro;
using UnityEngine;

public class QuestView : MonoBehaviour
{
    [SerializeField] private float _hideDuration;
    [SerializeField] private float _appearDuration;

    [SerializeField] private float _hideAmplitude = 1f;
    [SerializeField] private float _appearAmplitude = 1f;

    [SerializeField] private float _hidePeriod = 1f;
    [SerializeField] private float _appearPeriod = 1f;

    [SerializeField] private Ease _hideEase;
    [SerializeField] private Ease _appearEase;
    
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private RectTransform _target;
    
    public QuestData Data { get; private set; }
    private Tweener _tweener;

    private void Start()
    {
        if (this == false)
        {
            return;
        }

        if (gameObject.activeInHierarchy is false)
        {
            Destroy(gameObject);
            return;
        }
        
        StopAllCoroutines();
        StartCoroutine(CoAnimate(true));
    }
    
    private IEnumerator CoAnimate(bool fadein)
    {
        float hiddenX = transform.position.x - _target.rect.size.x;
        float appearedX = transform.position.x;

        float begin = fadein ? hiddenX : appearedX;
        float end = fadein ? appearedX : hiddenX;
        float duration = fadein ? _appearDuration : _hideDuration;
        float amplitude = fadein ? _appearAmplitude : _hideAmplitude;
        float period = fadein ? _appearPeriod : _hidePeriod;
        Ease ease = fadein ? _appearEase : _hideEase;
        
        _target.SetX(begin);
        _tweener?.Kill(true);
        _tweener = _target.DOMoveX(end, duration).SetEase(ease, amplitude, period);

        yield return new WaitForSeconds(duration);
    }

    public void SetData(QuestData data)
    {
        Debug.Assert(data);

        Data = data;
        _title.text = data.Title;
        _description.text = data.Description;
    }

    public void Clear()
    {
        Data = null;
        _title.text = "";
        _description.text = "";
    }

    public void DestroySelf()
    {
        if (this == false)
        {
            return;
        }

        if (gameObject.activeInHierarchy is false)
        {
            Destroy(gameObject);
            return;
        }
        
        StopAllCoroutines();
        StartCoroutine(CoAnimate(false));
        
        /*
         * 빌드에서 씬 넘어가기 전의 마지막 퀘스트가 Destroy가 안 되는 버그 발생
         * 확인 결과 Coroutine, Dotween 둘 다 정상 작동하지 않았음. (WaitForCompletion or Kill)
         * 그래서 그냥 UniTask로 지우도록 수정함
         * */
        _ = UniTask.Create(async () =>
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_hideDuration), DelayType.DeltaTime, PlayerLoopTiming.Update,
                    this.GetCancellationTokenOnDestroy());

                if (this)
                {
                    Destroy(gameObject);
                }
            }
            catch (OperationCanceledException)
            {
                if (this)
                {
                    Destroy(gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        });
    }

    private void OnDestroy()
    {
        _tweener?.Kill();
        _tweener = null;
    }
}