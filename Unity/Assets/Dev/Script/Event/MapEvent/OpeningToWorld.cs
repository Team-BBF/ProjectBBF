


using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using ProjectBBF.Persistence;
using UnityEngine;

public class OpeningToWorld : MonoBehaviour
{
    [SerializeField] private float _waitDuration;

    private void Awake()
    {
        UniTask
            .Delay(TimeSpan.FromSeconds(_waitDuration), DelayType.Realtime, PlayerLoopTiming.Update,
                this.GetCancellationTokenOnDestroy())
            .ContinueWith(async () =>
            {
                string scene = "World_Festival_Ch_0";

                _ = await SceneLoader.Instance.WorkDirectorAsync(false, "BlackAlpha");
                _ = await SceneLoader.Instance.LoadWorldAsync(scene);
                _ = await SceneLoader.Instance.LoadImmutableScenesAsync();
                _ = await SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlpha");
            }).Forget(Debug.LogError);
    }
    
}