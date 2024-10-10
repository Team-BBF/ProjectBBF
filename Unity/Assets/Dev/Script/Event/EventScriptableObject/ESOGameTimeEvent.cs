using System;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Event/Time/GameTimeEvent", fileName = "GameTimeEvent", order = 0)]
public class ESOGameTimeEvent : EventScriptableObjectT<GameTime>
{
    [System.Serializable]
    public enum Operation
    {
        //Equal,
        //NotEqual,
        GreaterThenEqual,
        Greater,
        Less,
        LessThenEqual,
        AllTicks
    }

    [SerializeField] private Operation _operation;
    [SerializeField] private GameTime _targetGameTime;

    public Operation OperationType => _operation;
    public GameTime TargetGameTime => _targetGameTime;

    private void OnValidate()
    {
        _targetGameTime.Min = Mathf.Clamp(_targetGameTime.Min - _targetGameTime.Min % 10, 0, 50);
    }
}