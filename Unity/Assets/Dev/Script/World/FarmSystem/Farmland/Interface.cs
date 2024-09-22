using System.Collections.Generic;
using UnityEngine;


public interface IFarmlandTile
{
    public ToolRequireSet[] RequireTools  { get; }
    public ItemData DropItem  { get; }
    public int DropItemCount { get; }
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
    public bool TryCultivateTile(Vector3 worldPos, CultivationTile tile);
}
public interface IBOFertilizerTile : IObjectBehaviour
{
    public bool PlantFertilizer(Vector3 worldPos, FertilizerTile tile);
}
public interface IBOSprinkleWaterTile : IObjectBehaviour
{
    public bool SprinkleWater(Vector3 worldPos);
}

public interface IBOCollectPlant : IObjectBehaviour
{
    public bool Collect(Vector3 worldPos, List<ItemData> itemList);
}