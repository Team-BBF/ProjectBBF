using System;
using ProjectBBF.Event;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectBBF.Input
{
    public class PIINDefault : PlayerInputInteract
    {
        
        public override void OnInit()
        {
        }

        public override void Update()
        {
            if (Owner.Interactor.CloserObject)
            {
                CollisionInteractionUtil
                    .CreateState()
                    .Bind<IBOInteractiveSingle>(OnInteractObject)
                    .Execute(Owner.Interactor.CloserObject.ContractInfo);
            }

            foreach (CollisionInteractionMono closerObject in Owner.Interactor.CloserObjects)
            {
                if (closerObject.ContractInfo is ObjectContractInfo info &&
                    info.TryGetBehaviour(out IBOInteractiveMulti interactive))
                {
                    interactive.UpdateInteract(Owner.Interaction);
                }
            }
        }

        public override void Release()
        {
        }
        private void OnInteractObject(IBOInteractiveSingle obj) => obj.UpdateInteract(Owner.Interaction);
    }
}