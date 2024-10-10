using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



[CreateAssetMenu(menuName = "ProjectBBF/FarmSystem/Farmland/CultivationTile", fileName = "New CultivationTile")]
public class CultivationTile : RuleTile, IFarmlandTile
{
    [SerializeField] private ItemData _dropItem;
    [SerializeField] private int _dropItemCount;
    [SerializeField] private ToolRequireSet[] _requireTools;

    public ToolRequireSet[] RequireTools => _requireTools;
    public ItemData DropItem => _dropItem;
    public int DropItemCount => _dropItemCount;
}
