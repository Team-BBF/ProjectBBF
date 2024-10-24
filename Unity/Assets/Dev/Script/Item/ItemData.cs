using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public struct ItemDataSerializedSet
{
    public ItemData Item;
    public int Count;
}

[Serializable]
public class ItemAudioInfo
{
    [SerializeField] private string _usingKey;
    [SerializeField] private string _mixerGroupKey;
    [SerializeField] private string _audioKey;

    public string UsingKey => _usingKey;

    public string MixerGroupKey => _mixerGroupKey;
    public string AudioKey => _audioKey;

    public bool HasAudio(string usingKey)
    {
        if (_usingKey == usingKey)
        {
            return string.IsNullOrEmpty(_audioKey) is false && string.IsNullOrEmpty(_mixerGroupKey) is false;
        }

        return false;
    } 
}

[CreateAssetMenu(menuName = "ProjectBBF/Data/Item/Default item", fileName = "New Default item data")]
public class ItemData : ScriptableObject
{
    [field: SerializeField, Header("아이템 키")]
    private string _itemKey;

    [field: SerializeField, Header("아이템 화면 이름")]
    private string _itemName;

    [field: SerializeField, Header("아이템 화면 설명")]
    private string _itemDescription;

    [field: SerializeField, Header("아이템 이미지")]
    private Sprite _itemSprite;

    [field: SerializeField, Header("최대 겹치기 개수")]
    private int _maxStackCount = 1;

    [field: SerializeField, Header("아이템 메타데이터")]
    private ItemTypeInfo _itemTypeInfo;

    [field: SerializeField, Header("건들 ㄴㄴ")]
    private ActionCategoryType _actionCategoryType;

    [field: SerializeField, Header("건들 ㄴㄴ")]
    private AnimationActorKey.Action _actionAnimationType;

    [field: SerializeField, Header("건들 ㄴㄴ")]
    private ItemAudioInfo[] _usingActionAudioInfos;

    [SerializeField]
    private float _usePrevWait;
    [SerializeField]
    private float _useAndWait;
    
    public string ItemKey => _itemKey;
    public string ItemName => _itemName;
    public string ItemDescription => _itemDescription;

    public Sprite ItemSprite => _itemSprite;
    public int MaxStackCount => Mathf.Max(1, _maxStackCount);
    public ItemTypeInfo Info => _itemTypeInfo;

    public ActionCategoryType ActionCategoryType => _actionCategoryType;

    public AnimationActorKey.Action ActionAnimationType => _actionAnimationType;
    public int ActionAnimationAniHash => AnimationActorKey.GetAniHash(ActionAnimationType);

    public ItemAudioInfo[] UseActionUsingActionAudioInfos => _usingActionAudioInfos;

    public float UsePrevWait => _usePrevWait;

    public float UseAndWait => _useAndWait;


    private void OnValidate()
    {
        _maxStackCount = Mathf.Max(1, _maxStackCount);
    }
}