using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class ScreenDirector : MonoBehaviour
{
    public abstract string Key { get; }
    public abstract UniTask Fadein();
    public abstract UniTask Fadeout();
    public abstract bool Enabled { get; set; }
}
