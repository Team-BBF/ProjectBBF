using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using MyBox;
using UnityEngine;

[RequireComponent(typeof(CollisionInteraction))]
public class CollectingObject : MonoBehaviour, IBOCollect
{
    #region Properties
    [field: SerializeField, InitializationField, MustBeAssigned, AutoProperty] 
    private SpriteRenderer _renderer;

    [field: SerializeField, InitializationField, MustBeAssigned, AutoProperty]
    private CollisionInteraction _interaction;

    [field: SerializeField, Separator("커스텀"), OverrideLabel("데이터"), InitializationField, MustBeAssigned, DisplayInspector]
    private CollectingObjectData _data;

    #endregion

    #region Getter/Setter
    public CollectingObjectData Data => _data;
    public CollisionInteraction Interaction => _interaction;
    #endregion

    private bool _isCollected;

    private void Awake()
    {
        var info = ObjectContractInfo.Create(() => gameObject);
        _interaction.SetContractInfo(info, this);

        info.AddBehaivour<IBOCollect>(this);
    }
    
    public List<ItemData> Collect()
    {
        if (_isCollected) return null;
        _isCollected = true;
        
        _renderer.sprite = _data.CollectedSprite;

        List<ItemData> list = new List<ItemData>(_data.DropItems.Count);

        foreach (CollectingObjectData.Item item in _data.DropItems)
        {
            for (int i = 0; i < item.Count; i++)
            {
                list.Add(item.Data);
            }
        }

        return list;
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