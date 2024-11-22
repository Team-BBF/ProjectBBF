using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Persistence;
using UnityEngine;

public class OpeningToWorld : MonoBehaviour
{
    [SerializeField] private float _waitDuration;

    private void Awake()
    {
        var task0 = UniTask.WaitUntil(
            () => Input.GetKeyDown(KeyCode.Return),
            PlayerLoopTiming.Update,
            this.GetCancellationTokenOnDestroy()
        );

        var task1 = UniTask.Delay(
            TimeSpan.FromSeconds(_waitDuration),
            DelayType.Realtime,
            PlayerLoopTiming.Update,
            this.GetCancellationTokenOnDestroy()
        );

        UniTask.WhenAny(task0, task1)
            .ContinueWith(async __ =>
            {
                string scene = "World_Festival_Ch_0";

                _ = await SceneLoader.Instance.WorkDirectorAsync(false, "BlackAlpha");
                _ = await SceneLoader.Instance.LoadWorldAsync(scene);
                _ = await SceneLoader.Instance.LoadImmutableScenesAsync();
                _ = await SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlpha");
            }).Forget(e=>{
                if (e is not OperationCanceledException)
                {
                    Debug.LogException(e);
                }
            });
    }
}