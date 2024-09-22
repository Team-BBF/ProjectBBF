using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ProjectBBF/FarmSystem/Farmland/FertilizerTile", fileName = "New FertilizerTile")]
public class FertilizerTile : Tile, IFarmlandTile
{
    [SerializeField] private ItemData _dropItem;
    [SerializeField] private int _dropItemCount;
    [SerializeField] private ToolRequireSet[] _requireTools;
    [SerializeField] private int _buffGrowingSpeed = 0;

    public ToolRequireSet[] RequireTools => _requireTools;
    public ItemData DropItem => _dropItem;
    public int DropItemCount => _dropItemCount;

    public int BuffGrowingSpeed => _buffGrowingSpeed;
}