using System;
using System.Runtime.Serialization;
using ProjectBBF.Persistence;
using UnityEngine;

[GameData]
[Serializable]
public class PlayerBlackboard
{
    [NonSerialized, Editable] private float _stemina;
    [NonSerialized, Editable] private float _maxStemina;

    [Editable] private int _energy = 50;
    [Editable] private int _maxEnergy = 50;

    private string _currentWorld;
    private Vector2 _currentPosition;

    public float Stemina
    {
        get => _stemina;
        set => _stemina = Mathf.Clamp(value, 0f, MaxStemina);
    }

    public float MaxStemina
    {
        get => _maxStemina;
        set => _maxStemina = value;
    }

    public int Energy
    {
        get => _energy;
        set => _energy = Mathf.Clamp(value, 0, MaxEnergy);
    }

    public int MaxEnergy
    {
        get => _maxEnergy;
        set => _maxEnergy = value;
    }

    public string CurrentWorld
    {
        get => _currentWorld;
        set => _currentWorld = value;
    }

    public Vector2 CurrentPosition
    {
        get => _currentPosition;
        set => _currentPosition = value;
    }
}