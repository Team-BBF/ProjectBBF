using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DefaultCollectMinigameController : CollectMinigameControllerBase<DefaultCollectMinigameData>
{
    protected override void OnGameInit()
    {
        base.OnGameInit();
        
        
    }

    protected override async UniTask OnTutorial()
    {
        await RunDialogue(Data.Tutorial);
    }

    protected override void OnGameBegin()
    {
    }

    protected override void OnGameRelease()
    {
        base.OnGameRelease();
    }

    protected override bool IsGameEnd()
    {
        return base.IsGameEnd();
    }

    protected override UniTask OnGameEnd(bool isRequestEnd)
    {
        foreach (var rewardItemSet in Data.Rewards)
        {
            Player.Inventory.Model.PushItem(rewardItemSet.Item, rewardItemSet.Count);
        }

        return UniTask.CompletedTask;
    }
}
