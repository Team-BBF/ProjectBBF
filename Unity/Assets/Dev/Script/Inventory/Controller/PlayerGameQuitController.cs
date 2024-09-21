using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerGameQuitController : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void GotoMainMenu()
    {
        SceneLoader.Instance.WorkDirectorAsync(false, "BlackAlpha")
            .ContinueWith(_ => SceneLoader.Instance.UnloadImmutableScenesAsync())
            .ContinueWith(_ => SceneLoader.Instance.LoadWorldAsync("World_MainMenu"))
            .ContinueWith(_ => SceneLoader.Instance.WorkDirectorAsync(true, "BlackAlpha"))
            .Forget();
    }
}
