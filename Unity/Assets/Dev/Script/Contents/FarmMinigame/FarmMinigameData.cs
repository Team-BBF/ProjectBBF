using System.Collections;
using System.Collections.Generic;
using DS.Core;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Minigame/Farm", fileName = "FarmMinigameData")]
public class FarmMinigameData : MinigameData
{
    [SerializeField] private float _lightIgnitionBeginWaveTime;
    [SerializeField] private float _lightIgnitionEndWaveTime;
    [SerializeField] private float _lightOffMoveSpeed;
    [SerializeField] private float _lightOnMoveSpeed;

    [SerializeField] private float _lightOnIntensity;
    [SerializeField] private float _lightOffIntensity;

    [SerializeField] private int _goalItemCount;
    [SerializeField] private ItemData _goalItem;

    [SerializeField] private DialogueContainer _dialogueTutorial;
    public float LightIgnitionBeginWaveTime => _lightIgnitionBeginWaveTime;

    public float LightIgnitionEndWaveTime => _lightIgnitionEndWaveTime;

    public float LightOffMoveSpeed => _lightOffMoveSpeed;

    public float LightOnMoveSpeed => _lightOnMoveSpeed;


    public float LightOnIntensity => _lightOnIntensity;

    public float LightOffIntensity => _lightOffIntensity;

    public int GoalItemCount => _goalItemCount;

    public ItemData GoalItem => _goalItem;

    public DialogueContainer DialogueTutorial => _dialogueTutorial;
}
