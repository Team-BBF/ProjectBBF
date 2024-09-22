using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Image _steminaFill;
    [SerializeField] private Image _energyFill;

    private PlayerBlackboard _blackboard;

    private void Start()
    {
        _blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
    }

    private void Update()
    {
        float value = _blackboard.Stemina / Mathf.Max(_blackboard.MaxStemina, 0.0001f);
        _steminaFill.fillAmount = value;
        
        value = _blackboard.Energy / Mathf.Max(_blackboard.MaxEnergy, 0.0001f);
        _energyFill.fillAmount = value;
    }
}
