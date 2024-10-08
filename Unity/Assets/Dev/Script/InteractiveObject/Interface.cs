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

public interface IBOBurn : IObjectBehaviour
{
    public UniTaskVoid DoFire();
}
public interface IBOCollect : IObjectBehaviour
{
    public List<ItemData> Collect();
}
public interface IBODestory : IObjectBehaviour
{
    public List<ItemData> Destory();
}
public interface IBODestoryTile : IObjectBehaviour
{
    
    /// <summary>
    /// 해당 좌표의 타일을 가능하다면 파괴하고 드랍 아이템을 반환함.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns>드랍된 아이템을 반환함. 단, 파괴에 실패했다면 null을 반환</returns>
    public List<ItemData> Destory(Vector3 worldPos, ItemTypeInfo itemTypeInfo);
}
public interface IBOPlantTile : IObjectBehaviour
{

    /// <summary>
    /// 해당 아이템을 농작 가능한 타일에 심음.
    /// 단, worldPos에 타일이 없거나, 심을 수 없는 타일/아이템 이라면 false를 반환.
    /// </summary>
    /// <param name="worldPos">타일의 world position</param>
    /// <returns>아이템을 타일에 심는데 성공했는지 여부</returns>
    public bool Plant(Vector3 worldPos, GrownDefinition definition);
}
public interface IBOCultivateTile : IObjectBehaviour
{
    public bool TryCultivateTile(Vector3 worldPos, FarmlandTile tile);
}