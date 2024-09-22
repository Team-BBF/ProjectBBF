using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class FishingView : MonoBehaviour
{
    [SerializeField] private Image _fillImage;

    private IEnumerator _co;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public async UniTask<float> Fishing(float pressTime, CancellationToken token = default)
    {
        _fillImage.fillAmount = 0f;
        gameObject.SetActive(true);

        pressTime = Mathf.Clamp(pressTime, 0.01f, pressTime);
        
        var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy())
            .Token;
        
        
        float t = 0f;
        float sign = 1;
        while (InputManager.Map.Player.Fishing.IsPressed())
        {

            if (t > pressTime)
            {
                t = pressTime;
                sign = -1;
            }
            else if(t < 0f)
            {
                t = 0f;
                sign = 1;
            }
            
            
            _fillImage.fillAmount = (t / pressTime);

            t += Time.deltaTime * sign;

            bool isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, cancelToken).SuppressCancellationThrow();

            if (isCancelled)
            {
                break;
            }
        }

        gameObject.SetActive(false);

        return t / pressTime;
    }
    
}
