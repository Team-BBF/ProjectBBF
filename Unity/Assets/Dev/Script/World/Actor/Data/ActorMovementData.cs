using System;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Actor/ActorMovementData", fileName = "NewActorMovementData")]
public class ActorMovementData : ScriptableObject
{
    [Serializable]
    public struct PathItem
    {
        [field: Header("PatrolPath 프리팹")]
        public GameObject Path;
        
        [field: Header("시간대")]
        public ESOGameTimeEvent ChangeTimeEvent;
    }

    [field: SerializeField, Header("이동 속도")]
    private float _movementSpeed;

    
    [field: SerializeField, Header("시간별 동선")]
    private List<PathItem> _paths;


    public float MovementSpeed => _movementSpeed;
    public IReadOnlyList<PathItem> Paths => _paths;

}