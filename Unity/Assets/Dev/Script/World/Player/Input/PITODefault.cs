using System;
using ProjectBBF.Event;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

                foreach (PlayerItemBehaviour behaviour in data.PlayerItemBehaviours)
                {
                    await behaviour.DoAction(Owner, data, Owner.GetCancellationTokenOnDestroy());
                }

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