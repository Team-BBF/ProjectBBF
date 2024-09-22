using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;

[Singleton(ESingletonType.Global)]
public class GameManager : MonoBehaviourSingleton<GameManager>
{
    public override void PostInitialize()
    {
        
    }

    public override void PostRelease()
    {
    }
}

