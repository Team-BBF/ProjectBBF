using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace  ProjectBBF.Event
{
public interface ICollisionInteraction : IEventSystemHandler
    {
        public event Action<ActorContractInfo> OnContractActor;
        public event Action<ObjectContractInfo> OnContractObject;
        public event Action<ClickContractInfo> OnContractClick;
        public event Action<ActorContractInfo> OnExitActor;
        public event Action<ObjectContractInfo> OnExitObject;
        public event Action<ClickContractInfo> OnExitClick;
        
        public LayerMask TargetLayerMask { get; }
        public bool ListeningOnly { get; }
        public bool DetectedOnly { get; }
        public BaseContractInfo ContractInfo { get; }

        public bool IsEnabled { get; set; }

        public T GetContractInfoOrNull<T>() where T : BaseContractInfo;

        public bool TryGetContractInfo<T>(out T info) where T : BaseContractInfo;
        public void Activate(BaseContractInfo info);
        public void DeActivate(BaseContractInfo info);

        public void ClearContractEvent();
        public object Owner { get; }
    }

    public abstract class CollisionInteractionMono : MonoBehaviour, ICollisionInteraction
    {
        public abstract object Owner { get; internal set; }
        public abstract event Action<ActorContractInfo> OnContractActor;
        public abstract event Action<ObjectContractInfo> OnContractObject;
        public abstract event Action<ClickContractInfo> OnContractClick;
        public abstract event Action<ActorContractInfo> OnExitActor;
        public abstract event Action<ObjectContractInfo> OnExitObject;
        public abstract event Action<ClickContractInfo> OnExitClick;
        public abstract LayerMask TargetLayerMask { get; }
        public abstract bool ListeningOnly { get; }
        public abstract bool DetectedOnly { get; }
        public abstract BaseContractInfo ContractInfo { get; internal set; }
        
        public abstract bool IsEnabled { get; set; }
        public abstract T GetContractInfoOrNull<T>() where T : BaseContractInfo;
        public abstract bool TryGetContractInfo<T>(out T info) where T : BaseContractInfo;
        public abstract void Activate(BaseContractInfo info);
        public abstract void DeActivate(BaseContractInfo info);

        public abstract void ClearContractEvent();
    }

}