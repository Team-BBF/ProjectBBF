﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Behaviour/Item/Pickaxe", fileName = "Bav_Item_Pickaxe")]
public class PIBPickaxe : PIBTwoStep
{
    //private Vector2 _targetPos;
    
    protected override async UniTask<ActionResult> PreAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        if (itemData && itemData.Info.Contains(ToolType.Pickaxe))
        {
            AudioManager.Instance.PlayOneShot("Player", "Player_Tool_Using_Pickaxe");
            AnimateLookAt(playerController, AnimationActorKey.Action.Pickaxe, true);
            //_targetPos = playerController.Interactor.IndicatedPosition;
            
            return ActionResult.Continue;
        }


        return ActionResult.Break;
    }

    protected override async UniTask<ActionResult> PostAction(PlayerController playerController, ItemData itemData,
        CancellationToken token = default)
    {
        await UniTask.CompletedTask;
        
        var interaction = playerController.Interactor.FindCloserObject();
        if (interaction == false) return ActionResult.Continue;

        return ActionResult.Continue;
    }

    protected override UniTask EndAction(PlayerController playerController, ItemData itemData, CancellationToken token = default)
    {
        return UniTask.CompletedTask;
    }
}