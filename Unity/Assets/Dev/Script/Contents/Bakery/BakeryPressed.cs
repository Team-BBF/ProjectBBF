using System;
using System.Collections;
using System.Linq;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BakeryPressed: BakeryFlowBehaviourBucket
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Transform _playPoint;
    [SerializeField] private AudioSource _audioSource;

    private void Start()
    {
        _panel.SetActive(false);
    }

    protected override void OnActivate(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) return;
        if (IsFullBucket is false) return;

        StartCoroutine(CoUpdate(pc));
    }

    protected override void OnEnter(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
    }

    protected override void OnExit(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        GameReset();
        StopAllCoroutines();
    }

    private IEnumerator CoUpdate(PlayerController pc)
    {
        float t = 0f;
        InputAction keyAction = InputManager.Map.Minigame.BakeryKeyPressed;
        
        _panel.SetActive(true);
        _fillImage.fillAmount = 0f;
        AnimationActorKey.Action aniAction;

        switch (ResolvorType)
        {
            case Resolvor.Dough:
                aniAction = AnimationActorKey.Action.Bakery_Knead;
                break;
            case Resolvor.Additive:
                aniAction = AnimationActorKey.Action.Bakery_Additive;
                break;
            case Resolvor.Baking:
                aniAction = AnimationActorKey.Action.Bakery_Oven;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        (ItemData failItem, ItemData resultItem, float duration, BakeryRecipeData recipeData) tuple = GetResolvedItem();

        pc.Blackboard.IsMoveStopped = true;
        pc.Blackboard.IsInteractionStopped = true;
        pc.transform.position = (Vector2)_playPoint.position;
        pc.MoveStrategy.ResetVelocity();
        pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(aniAction, AnimationActorKey.Direction.Down));


        if (ResolvorType == Resolvor.Dough)
        {
            var clip = AudioManager.Instance.GetAudio("SFX", "SFX_Bakery_Kneading").clip;
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        while (true)
        {
            if (keyAction.IsPressed() is false)
            {
                GameReset();
                
                pc.MoveStrategy.IsStopped = false;
                pc.MoveStrategy.ResetVelocity();
                pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Down), true);
                yield break;
            }

            _fillImage.fillAmount = t / tuple.duration;
            t += Time.deltaTime;

            if (t > tuple.duration)
            {
                break;
            }

            yield return null;
        }

        yield return null;
        
        pc.Blackboard.IsMoveStopped = false;
        pc.Blackboard.IsInteractionStopped = false;
        pc.MoveStrategy.ResetVelocity();
        pc.VisualStrategy.ChangeClip(AnimationActorKey.GetAniHash(AnimationActorKey.Action.Idle, AnimationActorKey.Direction.Down), true);
        
        _audioSource.Stop();
        
        if (tuple.resultItem is not null)
        {
            if (ResolvorType == Resolvor.Additive)
            {
                AudioManager.Instance.PlayOneShot("SFX", "SFX_Bakery_BakingComplete");
            }
            
            GameSuccess(tuple, pc);
        }
        else
        {
            GameFail(tuple, pc);
        }

        GameReset();
    }

    private void GameReset()
    {
        StopAllCoroutines();
        _panel.SetActive(false);
        _audioSource.Stop();
    }

    private void GameSuccess((ItemData failItem, ItemData resultItem, float duration, BakeryRecipeData recipeData) tuple, PlayerController pc)
    {
        if (tuple.resultItem == false) return;
        
        bool success = pc.Inventory.Model.PushItem(tuple.resultItem, 1);
        if (success is false) return;
        

        if (tuple.recipeData)
        {
            pc.RecipeBookPresenter.Model.Add(tuple.recipeData.Key);
        }
        
        ClearBucket();
    }
    private void GameFail((ItemData failItem, ItemData resultItem, float duration, BakeryRecipeData recipeData) tuple, PlayerController pc)
    {
        if (tuple.failItem == false) return;
        
        bool success = pc.Inventory.Model.PushItem(tuple.failItem, 1);
        if (success is false) return;
        
        ClearBucket();
    }
}