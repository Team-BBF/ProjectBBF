using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EndCreditToMainMenu : MonoBehaviour
{
    private void Awake()
    {
        ScreenManager.Instance.CurrentCursor = CursorType.None;
    }

    public void ToMainMenu()
    {
        if (SceneLoader.Instance)
        {
            SceneLoader.Instance
                .WorkDirectorAsync(false, "BlackAlphaEndCredit")
                .ContinueWith(_ => SceneLoader.Instance.LoadWorldAsync("World_MainMenu"))
                .ContinueWith(_ => SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlphaEndCredit"))
                .Forget(Debug.LogError);
        }
    }
}