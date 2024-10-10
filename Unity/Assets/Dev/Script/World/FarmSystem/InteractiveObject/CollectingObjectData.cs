using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/CollectingObjectData", fileName = "CollectingObjectData")]
public class CollectingObjectData : ScriptableObject
{
    [System.Serializable]
    public struct Item
    {
        public ItemData Data;
        public int Count;
    }


    [field: SerializeField, Header("수집 가능한 상태의 스프라이트")]
    private Sprite _defaultSprite;

    [field: SerializeField, Header("수집된 상태의 스프라이트")]
    private Sprite _collectedSprite;

    [field: SerializeField, Header("수집 가능한 최대 횟수")]
    private int _maxCollectCount;

    [field: SerializeField, Header("도구로만 획득 수집 가능?")]
    private bool _onlyTool;
    
    [field: SerializeField, Header("조건 도구")] 
    private ToolRequireSet _requireSet;

    [field: SerializeField, Header("수집했을 때 드랍하는 아이템 테이블")]
    private List<Item> _dropItems;

    public Sprite DefaultSprite => _defaultSprite;
    public Sprite CollectedSprite => _collectedSprite;

    public int MaxCollectCount => _maxCollectCount;

    public ToolRequireSet RequireSet => _requireSet;

    public IReadOnlyList<Item> DropItems => _dropItems;

    public bool OnlyTool => _onlyTool;
}