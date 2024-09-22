using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Singleton;
using UnityEngine;

[Singleton(ESingletonType.Global)]
public class InputManager : IGeneralSingleton
{
    private static DefaultKeymap _keyMap;

    public void Initialize()
    {
        _keyMap = new DefaultKeymap();
        _keyMap.Enable();
    }

    public void Release()
    {
        _keyMap.Dispose();
    }


    public static DefaultKeymap Map => _keyMap;
}
