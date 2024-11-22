using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EndCreditToMainMenu : MonoBehaviour
{
    private bool _isMoved;
    private void Awake()
    {
        ScreenManager.Instance.CurrentCursor = CursorType.None;

        UniTask.WaitUntil(
            () => Input.GetKeyDown(KeyCode.Return),
            PlayerLoopTiming.Update,
            this.GetCancellationTokenOnDestroy()
        ).ContinueWith(() =>
        {
            if (_isMoved) return;
            ToMainMenu();
        }).Forget(e =>
        {
            if (e is not OperationCanceledException)
            {
                Debug.LogException(e);
            }
        });
    }

    public void ToMainMenu()
    {
        if (SceneLoader.Instance)
        {
            _isMoved = true;
            SceneLoader.Instance
                .WorkDirectorAsync(false, "BlackAlphaEndCredit")
                .ContinueWith(_ => SceneLoader.Instance.LoadWorldAsync("World_MainMenu"))
                .ContinueWith(_ => SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlphaEndCredit"))
                .Forget(Debug.LogError);
        }
    }
}