using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


[GameData]
[Serializable]
public class PlayerBlackboard : ISaveLoadNotification
{
    
    [NonSerialized, Editable] private float _stemina;
    [NonSerialized, Editable] private float _maxStemina;

    [Editable] private int _energy = 50;
    [Editable] private int _maxEnergy = 50;

    [SerializeField, Editable] private string _currentWorld;
    [SerializeField, Editable] private Vector2 _currentPosition;
    [SerializeField, Editable] private bool _isPositionSaved;

    [SerializeField, Editable] private int _money = 500;

    private PlayerInventoryPresenter _inventory;

    public PlayerInventoryPresenter Inventory
    {
        get => _inventory;
        set => _inventory = value;
    }
    
    public bool IsMoveStopped { get; set; }
    public bool IsInteractionStopped { get; set; }
    public bool IsFishingStopped { get; set; } = true;

    [SerializeField, Editable] private string _prevWorld;

    public event Action<PlayerBlackboard> OnSaved;

    public float Stemina
    {
        get => 999f;
        //get => _stemina;
        set => _stemina = Mathf.Clamp(value, 0f, MaxStemina);
    }

    public float MaxStemina
    {
        get => 999f;
        //get => _maxStemina;
        set => _maxStemina = value;
    }

    public int Energy
    {
        get => 999;
        //get => _energy;
        set => _energy = Mathf.Clamp(value, 0, MaxEnergy);
    }

    public int MaxEnergy
    {
        get => 999;
        //get => _maxEnergy;
        set => _maxEnergy = value;
    }

    public string PrevWorld
    {
        get => _prevWorld;
        private set => _prevWorld = value;
    }
    public string CurrentWorld
    {
        get => _currentWorld;
        set
        {
            PrevWorld = _currentWorld;
            _currentWorld = value;
        }
    }

    public bool IsPositionSaved => _isPositionSaved;

    public Vector2 CurrentPosition
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            _isPositionSaved = true;
        }
    }

    public int Money
    {
        get => _money;
        set => _money = Mathf.Clamp(value, 0, int.MaxValue);
    }

    public void OnSavedNotify()
    {
        OnSaved?.Invoke(this);
    }

    public void OnLoadedNotify()
    {
    }
}