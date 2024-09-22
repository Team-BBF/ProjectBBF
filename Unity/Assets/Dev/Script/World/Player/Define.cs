using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EPlayerControlState : int
{
    None = 0,
    Normal,
    Battle,
    Builder,
    Collection,
    
    Frist = None,
    Last = Collection
}

public interface IPlayerStrategy
{
    public void Init(PlayerController controller);
}