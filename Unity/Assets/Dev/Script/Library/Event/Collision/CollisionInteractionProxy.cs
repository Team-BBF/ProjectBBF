using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ProjectBBF.Event
{
    public class CollisionInteractionProxy : CollisionInteractionMono
    {
        public override object Owner
        {
            get => MainInteraction.Owner;
            internal set => MainInteraction.Owner = value;
        }
        
        internal CollisionInteraction MainInteraction;

        public override event Action<ActorContractInfo> OnContractActor
        {
            add => MainInteraction.OnContractActor += value;
            remove => MainInteraction.OnContractActor -= value;
        }
        public override event Action<ObjectContractInfo> OnContractObject
        {
            add => MainInteraction.OnContractObject += value;
            remove => MainInteraction.OnContractObject -= value;
        }
        public override event Action<ClickContractInfo> OnContractClick
        {
            add => MainInteraction.OnContractClick += value;
            remove => MainInteraction.OnContractClick -= value;
        }
        public override event Action<ActorContractInfo> OnExitActor
        {
            add => MainInteraction.OnExitActor += value;
            remove => MainInteraction.OnExitActor -= value;
        }
        public override event Action<ObjectContractInfo> OnExitObject
        {
            add => MainInteraction.OnExitObject += value;
            remove => MainInteraction.OnExitObject -= value;
        }
        public override event Action<ClickContractInfo> OnExitClick
        {
            add => MainInteraction.OnExitClick += value;
            remove => MainInteraction.OnExitClick -= value;
        }

        public override LayerMask TargetLayerMask => MainInteraction.TargetLayerMask;

        public override bool ListeningOnly => MainInteraction.ListeningOnly;
        public override bool DetectedOnly => MainInteraction.DetectedOnly;

        public override BaseContractInfo ContractInfo
        {
            get=> MainInteraction.ContractInfo;
            internal set => MainInteraction.ContractInfo = value;
        }
        
        public override bool IsEnabled
        {
            get => MainInteraction.IsEnabled;
            set => MainInteraction.IsEnabled = value;
        }

        public override T GetContractInfoOrNull<T>()
            => MainInteraction.GetContractInfoOrNull<T>();

        public override bool TryGetContractInfo<T>(out T info)
        {
            if (MainInteraction.TryGetContractInfo<T>(out T i))
            {
                info = i;
                return true;
            }
            
            info = null;
            return false;
        }

        public override void Activate(BaseContractInfo info)
            => MainInteraction.Activate(info);

        public override void DeActivate(BaseContractInfo info)
            => MainInteraction.DeActivate(info);

        public override void ClearContractEvent()
            => MainInteraction.ClearContractEvent();



        private CollisionBridge _collisionBridge;
        private void Start()
        {
            _collisionBridge = Singleton.Singleton.GetSingleton<EventController>().GetBridge<CollisionBridge>();
        }
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (CollisionInteractionUtil.OnCollision(other.collider, this, true, out var com))
            {
                _collisionBridge.Push(MainInteraction, com);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (CollisionInteractionUtil.OnCollision(other, this, true, out var com))
            {
                _collisionBridge.Push(MainInteraction, com);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            CollisionInteractionUtil.OnCollision(other, this, false, out var com);
        }
        private void OnCollisionExit2D(Collision2D other)
        {
            CollisionInteractionUtil.OnCollision(other.collider, this, false, out var com);
        }
    }
}