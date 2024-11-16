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
                    .Bind<IBOInteractive>(OnInteractObject)
                    .Execute(Owner.Interactor.CloserObject.ContractInfo);
            }
        }

        public override void Release()
        {
        }
        private void OnInteractObject(IBOInteractive obj) => obj.UpdateInteract(Owner.Interaction);
    }
}