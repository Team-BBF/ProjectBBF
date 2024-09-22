using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ProjectBBF/FarmSystem/Farmland/PlantTile", fileName = "New Plant tile")]
public class PlantTile : AnimatedTile, IFarmlandTile
{
    [SerializeField] public Sprite _defaultEditorSprite;
    [SerializeField] private ItemData _dropItem;
    [SerializeField] private int _dropItemCount;
    [SerializeField] private ToolRequireSet[] _requireTools;

    public ToolRequireSet[] RequireTools => _requireTools;

    public ItemData DropItem => _dropItem;

    public int DropItemCount => _dropItemCount;

    public Sprite DefaultEditorSprite
    {
        get => _defaultEditorSprite;
        set => _defaultEditorSprite = value;
    }
}
