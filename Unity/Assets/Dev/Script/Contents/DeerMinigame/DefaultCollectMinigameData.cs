using System.Collections;
using System.Collections.Generic;
using DS.Core;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Minigame/Default Collect", fileName = "New default collect")]
public class DefaultCollectMinigameData : CollectMinigameDataBase
{
    [SerializeField] private DialogueContainer _tutorial;


    public DialogueContainer Tutorial => _tutorial;
}
