using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

namespace ProjectBBF.Event
{
    public abstract class EventScriptableObject : ScriptableObject
    {
        public bool IsTriggered { get; set; }

        public virtual void Release()
        {
            IsTriggered = false;
        }
    }

    public abstract class EventScriptableObjectVoid : EventScriptableObject
    {
        public event Action OnSignal;
        public void Signal()
        {
            if (IsTriggered) return;
            IsTriggered = true;

            OnSignal?.Invoke();
        }

        public async UniTask WaitAsync(CancellationToken token = default)
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
        }

        public override void Release()
        {
            base.Release();
        }
    }
    public abstract class EventScriptableObjectT<T1> : EventScriptableObject
    {
        public event Action<T1> OnSignal;
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
            base.Release();
            _arg1 = default;
        }
    }
}