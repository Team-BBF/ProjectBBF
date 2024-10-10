using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public enum EPlayerCollectingAnimation : int
{
    None = 0,
    Hand,
    Shovels
}

public interface IBOCollect : IObjectBehaviour
{
    /// <summary>
    /// Collect에 성공하면 List를 반환하고, 실패하면 null을 반환합니다.
    /// </summary>
    /// <returns></returns>
    [CanBeNull] public List<ItemData> Collect();
}
public interface IBACollect : IActorBehaviour
{
    /// <summary>
    /// Collect에 성공하면 List를 반환하고, 실패하면 null을 반환합니다.
    /// </summary>
    /// <returns></returns>
    [CanBeNull] public List<ItemData> Collect();
}

public interface IBOCollectTool : IObjectBehaviour
{
    public bool CanCollect(ToolRequireSet toolSet);
    
    /// <summary>
    /// Collect에 성공하면 List를 반환하고, 실패하면 null을 반환합니다.
    /// </summary>
    /// <returns></returns>
    [CanBeNull] public List<ItemData> Collect();
}
public interface IBACollectTool : IActorBehaviour
{
    public bool CanCollect(ToolRequireSet toolSet);
    
    /// <summary>
    /// Collect에 성공하면 List를 반환하고, 실패하면 null을 반환합니다.
    /// </summary>
    /// <returns></returns>
    [CanBeNull] public List<ItemData> Collect();
}
