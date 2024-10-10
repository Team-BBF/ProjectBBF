using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Actor/FavorabilityData", fileName = "NewFavorabilityData")]
public class FavorabilityData : ScriptableObject
{
    [field: SerializeField, Foldout("값"), Header("Npc(actor) 키")] 
    private string _actorKey;
    
    [field: SerializeField, Foldout("값"), Header("Npc 화면 출력 이름")] 
    private string _actorName;
    
    [field: SerializeField, Foldout("값"), Header("Npc의 기본 포트레이트 키")] 
    private string _defaultPortraitKey;
    
    
    [field: SerializeField, Foldout("참조"), OverrideLabel("호감도 이벤트 테이블")] 
    private FavorabilityEvent _favorabilityEvent;
    
    [field: SerializeField, Foldout("참조"), OverrideLabel("초상화 테이블")]
    private PortraitTable _portraitTable;

    public string ActorKey => _actorKey;

    public string ActorName => _actorName;

    public string DefaultPortraitKey => _defaultPortraitKey;

    public FavorabilityEvent FavorabilityEvent => _favorabilityEvent;
    public PortraitTable PortraitTable => _portraitTable;
}