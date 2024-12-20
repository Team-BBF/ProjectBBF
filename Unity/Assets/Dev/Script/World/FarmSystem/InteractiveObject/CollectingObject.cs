using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using MyBox;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CollisionInteraction))]
public class CollectingObject : MonoBehaviour, IBOInteractiveSingle
{
    #region Properties
    [field: SerializeField, InitializationField, MustBeAssigned, AutoProperty] 
    private SpriteRenderer _renderer;

    [field: SerializeField, InitializationField, MustBeAssigned, AutoProperty]
    private CollisionInteraction _interaction;
    
    [SerializeField] private UnityEvent _onStateRecovered;
    [SerializeField] private UnityEvent _onStateCollected;

    [field: SerializeField, Separator("커스텀"), OverrideLabel("데이터"), InitializationField, MustBeAssigned]
    private CollectingObjectData _data;

    #endregion

    #region Getter/Setter
    public CollectingObjectData Data => _data;
    public CollisionInteraction Interaction => _interaction;
    #endregion

    private bool _isCollected;

    public bool IsCollected
    {
        get => _isCollected;
        set
        {
            
            _isCollected = value;
            
            if (value)
            {
                _renderer.sprite = _data.CollectedSprite;
                _onStateCollected?.Invoke();
            }
            else
            {
                _renderer.sprite = _data.DefaultSprite;
                _onStateRecovered?.Invoke();
            }
        }
    }

    private void Awake()
    {
        var info = ObjectContractInfo.Create(() => gameObject);
        _interaction.SetContractInfo(info, this);

        info.AddBehaivour<IBOInteractiveSingle>(this);
    }

    private void Start()
    {
        var obj = PersistenceManager.Instance.LoadOrCreate<CollectPersistenceObject>(Data.PersistenceKey);
        PersistenceManager.Instance.OnGameDataLoaded += OnLoaded;
        obj.Object = this;
        
        if (obj.Saved is false)
        {
            obj.Saved = true;
        }
        else
        {
            obj.Load(this);
        }
    }

    private void OnLoaded(PersistenceManager inst)
    {
        if (this == false)
        {
            inst.OnGameDataLoaded -= OnLoaded;
            return;
        }
        
        var obj = PersistenceManager.Instance.LoadOrCreate<CollectPersistenceObject>(Data.PersistenceKey);
        obj.Object = this;
        obj.Load(this);
    }

    private void OnDestroy()
    {
        if (PersistenceManager.Instance)
        {
            var obj = PersistenceManager.Instance.LoadOrCreate<CollectPersistenceObject>(Data.PersistenceKey);
            PersistenceManager.Instance.OnGameDataLoaded -= OnLoaded;
            obj.Object = this;
            obj.Save(this);
        }
    }

    public void UpdateInteract(CollisionInteractionMono caller)
    {
        if (caller.Owner is not PlayerController pc) return;
        if (InputManager.Map.Player.Interaction.triggered is false) return;

        Collect(pc);
    }

    public void Collect(PlayerController pc)
    {
        if (_isCollected) return;
        _isCollected = true;

        Vector2 dir = Interaction.transform.position - pc.transform.position;
        pc.Interactor.WaitForPickupAnimation(dir);
        
        _renderer.sprite = _data.CollectedSprite;

        AudioManager.Instance.PlayOneShot("Player", "Player_Getting_Item");

        foreach (CollectingObjectData.Item item in _data.DropItems)
        {
            pc.Inventory.Model.PushItem(item.Data, item.Count);
        }

        if (_isCollected)
        {
            _onStateCollected.Invoke();
        }
    }

    [ButtonMethod]
    private void ApplyData()
    {
        if (_renderer == false) return;
        if (Data == false) return;
        if (Data.DefaultSprite == false) return;

        _renderer.sprite = _data.DefaultSprite;
    }
}