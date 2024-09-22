using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Player/MovementData", fileName = "SampleMovementData")]

public class PlayerMovementData : ScriptableObject
{
    [field: SerializeField, Foldout("이동 관련 설정"), OverrideLabel("걷기 속도(m/s)")]
    private float _movementSpeed;
    [field: SerializeField, Foldout("이동 관련 설정"), OverrideLabel("달리기 속도(m/s)")]
    private float _sprintSpeed;
    
    [field: SerializeField, Foldout("스텟 관련 설정"), OverrideLabel("스테미나")]
    private float _defaultStemina;
    [field: SerializeField, Foldout("스텟 관련 설정"), OverrideLabel("스테미나 채워지는 속도(초)")]
    private float _steminaIncreasePerSec;
    [field: SerializeField, Foldout("스텟 관련 설정"), OverrideLabel("스테미나 줄어드는 속도(초)")]
    private float _steminaDecreasePerSec;
    [field: SerializeField, Foldout("스텟 관련 설정"), OverrideLabel("스테미나 채우기 시작 대기 속도(초)")]
    private float _steminaIncreaseWaitDuration;

    public float MovementSpeed => _movementSpeed;

    public float SprintSpeed => _sprintSpeed;

    public float DefaultStemina => _defaultStemina;

    public float SteminaIncreasePerSec => _steminaIncreasePerSec;
    public float SteminaDecreasePerSec => _steminaDecreasePerSec;

    public float SteminaIncreaseWaitDuration => _steminaIncreaseWaitDuration;
}
