using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class MinigamePlayer : MonoBehaviour
{
    private IMinigame _minigame;

    private void Awake()
    {
        _minigame = GetComponent<IMinigame>();

        SceneLoader.Instance.WorldPostLoaded += Play;
    }

    private void OnDestroy()
    {
        if(SceneLoader.Instance)
            SceneLoader.Instance.WorldPostLoaded -= Play;
    }

    [ButtonMethod]
    private void OnPlay()
    {
        Play("");
    }
    
    private void Play(string _)
    {
        if (_minigame is not null)
        {
            _minigame.PlayGame();
        }
    }
}
