using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Item/Plant", fileName = "New Plant item data")]
public class PlantItemData : ItemData
{
    [SerializeField] private GrownDefinition _definition;

    public GrownDefinition Definition => _definition;
}
