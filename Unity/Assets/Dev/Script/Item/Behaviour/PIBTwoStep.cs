using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;
public abstract class PIBTwoStep : PlayerItemBehaviour
{
    public enum ActionResult
    {
        Continue,
        Break,
    }
    
    [SerializeField] private float _durationForWaitTiming;
    [SerializeField] private float _durationForWaitTotal;

    public float WaitStep0 => _durationForWaitTiming;

    public float WaitStep1
    {
        get
        {
            float endDuration = _durationForWaitTotal - _durationForWaitTiming;
            endDuration = Mathf.Max(endDuration, 0f);
            
            return endDuration;
        }
    }

    protected abstract UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData, CancellationToken token = default);
    protected abstract UniTask<ActionResult> PostAction(PlayerController playerController, ItemData itemData, CancellationToken token = default);
    protected abstract UniTask EndAction(PlayerController playerController, ItemData itemData, CancellationToken token = default);

    public sealed override async UniTask DoAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        try
        {
            ActionResult result = await PreAction(playerController, itemData, token);
            if (result == ActionResult.Break) return;

            await playerController.Interactor.WaitForSecondAsync(WaitStep0, token);

            result = await PostAction(playerController, itemData, token);
            if (result == ActionResult.Break) return;

            await playerController.Interactor.WaitForSecondAsync(WaitStep1, token);
            
            await EndAction(playerController, itemData, token);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
    }

}