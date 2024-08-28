using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum EPlayerCollectingAnimation : int
{
    None = 0,
    Hand,
    Shovels
}

public interface IBOCollect : IObjectBehaviour
{
    public List<ItemData> Collect();
}
