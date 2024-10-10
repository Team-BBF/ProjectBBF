using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ProjectBBF/Data/Item/Fertilizer", fileName = "New Fertilizer item data")]
public class FertilizerItemData : ItemData
{
    [SerializeField] private FertilizerTile _targetTile;

    public FertilizerTile TargetTile => _targetTile;
}