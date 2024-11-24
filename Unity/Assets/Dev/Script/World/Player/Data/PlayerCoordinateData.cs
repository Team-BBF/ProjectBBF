using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Player/CoordnateData", fileName = "new CoordinateData")]

public class PlayerCoordinateData : ScriptableObject
{
    [SerializeField] private Vector2 _sideOffset;
    [SerializeField] private Vector2 _upOffset;
    [SerializeField] private Vector2 _downOffset;
    [SerializeField] private float _radius;
    [SerializeField] private float _maxDistance;

    public float MaxDistance => _maxDistance;

    public Vector2 SideOffset => _sideOffset;

    public Vector2 UpOffset => _upOffset;

    public Vector2 DownOffset => _downOffset;

    public float Radius => _radius;
}