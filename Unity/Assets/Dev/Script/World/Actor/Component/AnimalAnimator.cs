using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AnimalAnimator : ActorComponent
{
    [Serializable]
    private struct ValueSlider
    {
        public string Key;
        public float Value;
        public RangedFloat WairDuration;
    }

    [SerializeField] private RangedFloat _startDelay;
    [SerializeField, InitializationField] private List<ValueSlider> _slider;
    
    private Animator _animator;
    private ActorMove _move;
    
    
    
    public void Init(Actor actor)
    {
        _animator = actor.Animator;
        _move = actor.MoveStrategy;

        CheckValid();

        StartCoroutine(CoUpdate());
    }

    private void CheckValid()
    {
        float total = 0f;
        foreach (var slider in _slider)
        {
            total += slider.Value;
        }

        if (total > 1f)
        {
            Debug.LogWarning("Slider의 총 합 값이 1을 초과합니다.");
        }
    }

    public void PlayHit(CollectState state)
    {
        if (state is CollectState.Normal) return;
        
        _animator.SetTrigger("Hit");

        foreach (ValueSlider slider in _slider)
        {
            _animator.ResetTrigger(slider.Key);
        }
        
    }

    private IEnumerator CoUpdate()
    {
        yield return new WaitForSeconds(Random.Range(_startDelay.Min, _startDelay.Max));
        
        List<(WaitForSeconds waits, string key, float probability)> waits =_slider.Select(x=> (new WaitForSeconds(Random.Range(x.WairDuration.Min, x.WairDuration.Max)), x.Key, x.Value)).ToList();
        List<float> probabilities = _slider.Select(x => x.Value).ToList();

        while (true)
        {
            if (_move.IsMoving)
            {
                yield return null;
                continue;
            }
            
            if(probabilities.Count == 0) 
            {
                yield return null;
                continue;
            }
            
            int index = ProbabilityHelper.GetRandomIndex(probabilities);
            if (index == -1)
            {
                Debug.LogError("확률 리스트가 비어있음: " + _move.Interaction.Owner);
                break;
            }

            var wait = waits[index];

            if (_animator)
            {
                _animator.SetTrigger(wait.key);
            }
                
            yield return wait.waits;
        }
    }
    
    
}
