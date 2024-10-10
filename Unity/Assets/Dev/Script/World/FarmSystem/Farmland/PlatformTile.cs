using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ProjectBBF/FarmSystem/Farmland/PlatformTile", fileName = "New PlatformTile")]
public class PlatformTile : Tile, IFarmlandTile
{
    private static ToolRequireSet[] _requireTools = new []{new ToolRequireSet(ToolType.Hoe, ToolRank.R1)};
    public ToolRequireSet[] RequireTools => _requireTools;
    public ItemData DropItem => null;
    public int DropItemCount => 0;

}