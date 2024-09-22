using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

namespace ProjectBBF.Event
{
    public abstract class EventScriptableObject : ScriptableObject
    {
        public abstract void Release();
    }

    public abstract class EventScriptableObjectT<T1> : EventScriptableObject
    {
        public event Action<T1> OnSignal;

        public bool IsTriggered { get; set; }
        T1 _arg1;

        public void Signal(T1 arg1)
        {
            if (IsTriggered) return;
            IsTriggered = true;

            _arg1 = arg1;
            OnSignal?.Invoke(arg1);
        }

        public async UniTask<T1> WaitAsync(CancellationToken token = default)
        {
            while (IsTriggered is false && token.IsCancellationRequested is false)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            return _arg1;
        }

        public override void Release()
        {
            IsTriggered = false;
        }
    }
}