using System;
using System.Collections.Generic;
using ProjectBBF.Event;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ProjectBBF.Input
{
    public class PITODefault : PlayerInputTool
    {
        public override void OnInit()
        {
            
        }

        public override void Update()
        {
            if (InputManager.Map.Player.UseTool.triggered)
            {
                _ = OnToolAction();
            }
        }

        public override void Release()
        {
        }
        

        public async UniTask<bool> OnToolAction()
        {
            try
            {
                ItemData data = Owner.Inventory.CurrentItemData;
                if (data == false) return false;

                IsUsingTool = true;

                List<UniTask> tasks = new List<UniTask>(data.PlayerItemBehaviours.Count);
                
                foreach (PlayerItemBehaviour behaviour in data.PlayerItemBehaviours)
                {
                    UniTask task = behaviour.DoAction(Owner, data, Owner.GetCancellationTokenOnDestroy());
                    tasks.Add(task);
                }

                _ = await UniTask.WhenAll(tasks).SuppressCancellationThrow();
                
                await UniTask.Yield(PlayerLoopTiming.Update, Owner.GetCancellationTokenOnDestroy());
                IsUsingTool = false;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Debug.LogException(e);
            }

            
            IsUsingTool = false;

            return false;
        }

    }
}