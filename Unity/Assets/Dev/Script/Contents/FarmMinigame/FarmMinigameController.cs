using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DS.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FarmMinigameController : MinigameBase<FarmMinigameData>
{
    [SerializeField] private FarmlandManager _farmlandManager;
    
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;

    [SerializeField] private Light2D _light;
    [SerializeField] private Transform _lightOnPoint;
    [SerializeField] private Transform _lightOffPoint;

    private int _currentItemCount = 0;

    protected override void Awake()
    {
        base.Awake();
        _light.gameObject.SetActive(false);
    }

    private IEnumerator CoUpdate()
    {
        Debug.Assert(Data is not null);
        
        _light.gameObject.SetActive(true);
        _light.intensity = Data.LightOffIntensity;
        _light.transform.position = (Vector2)_lightOffPoint.position;

        while (true)
        {
            yield return new WaitForSeconds(Data.LightIgnitionBeginWaveTime);
            
            Sequence sequence = DOTween.Sequence();
            sequence.Join
            (
                _light.transform.DOMove((Vector2)_lightOnPoint.position, Data.LightOnMoveSpeed).SetEase(Ease.Linear)
            );
            sequence.Join
            (
                DOTween.To(()=>_light.intensity, x => _light.intensity = x, Data.LightOnIntensity, 0.25f)
            );
            sequence.SetId(this);
            
            yield return new WaitForSeconds(Data.LightIgnitionEndWaveTime);
            
            _farmlandManager.GrowUp(1);
            
            sequence = DOTween.Sequence();
            sequence.Join
            (
                _light.transform.DOMove((Vector2)_lightOffPoint.position, Data.LightOffMoveSpeed).SetEase(Ease.Linear)
            );
            sequence.Join
            (
                DOTween.To(()=>_light.intensity, x => _light.intensity = x, Data.LightOffIntensity, 0.25f)
            );
            sequence.SetId(this);
            
            yield return sequence;
        }
    }


    private void OnItemCount(ItemData itemData, int count, GridInventoryModel model)
    {
        var gameData = Data as FarmMinigameData;
        
        if (itemData == false) return;
        if (itemData == gameData.GoalItem)
        {
            _currentItemCount += count;
        }
    }
    protected override void OnGameInit()
    {
        var data = Data as FarmMinigameData;

        _light.transform.position = _lightOffPoint.position;
        _light.intensity = data.LightOffIntensity;
        Player.Inventory.Model.OnPushItem += OnItemCount;
    }

    protected override async UniTask OnTutorial()
    {
        var data = Data as FarmMinigameData;
        await RunDialogue(data.DialogueTutorial);
    }

    protected override void OnGameBegin()
    {
        StartCoroutine(CoUpdate());
    }

    protected override void OnGameRelease()
    {
        _light.gameObject.SetActive(false);
        DOTween.Kill(this);
        _farmlandManager.ResetFarm();
        DialogueController.Instance.ResetDialogue();
        Player.Inventory.Model.OnPushItem -= OnItemCount;
        _currentItemCount = 0;
        
        StopAllCoroutines();
    }

    protected override bool IsGameEnd()
    {
        var data = Data as FarmMinigameData;

        return _currentItemCount >= data.GoalItemCount;
    }
    
}
