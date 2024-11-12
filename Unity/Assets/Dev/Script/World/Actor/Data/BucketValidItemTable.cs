using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;


[CreateAssetMenu(menuName = "ProjectBBF/Data/Actor/BucketValidItemTable", fileName = "New BucketValidItemTable")]
public class BucketValidItemTable : ScriptableObject
{
    [SerializeField] private List<ItemData> _list;

    public IReadOnlyList<ItemData> List => _list;
}