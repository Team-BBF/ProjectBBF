using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectBBF.Event
{
    public abstract class BaseInteractionStateCallback
    {
        public Type Type;
        public abstract bool Invoke(IBaseBehaviour behaviour);
    }
    
    public sealed class InteractionStateCallback<T> : BaseInteractionStateCallback
        where T : IBaseBehaviour
    {
        public Action<T> Callback;
        public override bool Invoke(IBaseBehaviour behaviour)
        {
            if (behaviour is T b)
            {
                Callback?.Invoke(b);
                return true;
            }

            return false;
        }
    }
    public sealed class SelectInteractionStateCallback<T> : BaseInteractionStateCallback
        where T : IBaseBehaviour
    {
        public Func<T, bool> Callback;
        public override bool Invoke(IBaseBehaviour behaviour)
        {
            if (behaviour is T b && Callback is not null)
            {
                return Callback!.Invoke(b);
            }

            return false;
        }
    }

    public sealed class SelectInteractionState
    {
        private List<BaseInteractionStateCallback> _callbacks = new(2);

        public SelectInteractionState Bind<T>(Func<T, bool> callback)
            where T : IBaseBehaviour
        {
            _callbacks.Add(new SelectInteractionStateCallback<T>()
            {
                Callback = callback,
                Type = typeof(T)
            });
            
            return this;
        }

        public SelectInteractionState Execute(BaseContractInfo info, out bool executedAny)
        {
            Debug.Assert(info != null, "ContractInfo must be not null");

            executedAny = Inner_Execute(info);
            
            return this;
        }

        public SelectInteractionState Execute<TContractInfo>(GameObject gameObject, out bool executedAny) 
            where TContractInfo : BaseContractInfo
        {
            executedAny = false;
            Debug.Assert(gameObject != null, "GameObject must be not null");
            
            if (gameObject.TryGetComponent<CollisionInteraction>(out var com) &&
                com.TryGetContractInfo<TContractInfo>(out var info))
            {
                executedAny = Inner_Execute(info);
            }
            else
            {
                Debug.Assert(false, "failed acquire contractInfo");
            }

            return this;
        }

        private bool Inner_Execute(BaseContractInfo info)
        {
            foreach (BaseInteractionStateCallback callback in _callbacks)
            {
                var behaviour = info.GetBehaviourOrNull(callback.Type);

                if (behaviour != null)
                {
                    if (callback.Invoke(behaviour)) return true;
                }
            }

            return false;
        }
    }
    
    public sealed class InteractionState
    {
        private List<BaseInteractionStateCallback> _callbacks = new(2);

        public InteractionState Bind<T>(Action<T> callback)
            where T : IBaseBehaviour
        {
            _callbacks.Add(new InteractionStateCallback<T>()
            {
                Callback = callback,
                Type = typeof(T)
            });
            
            return this;
        }

        public InteractionState Execute(BaseContractInfo info)
        {
            Debug.Assert(info != null, "ContractInfo must be not null");

            Inner_Execute(info);
            
            return this;
        }

        public InteractionState Execute<TContractInfo>(GameObject gameObject) 
            where TContractInfo : BaseContractInfo
        {
            Debug.Assert(gameObject != null, "GameObject must be not null");
            
            if (gameObject.TryGetComponent<CollisionInteraction>(out var com) &&
                com.TryGetContractInfo<TContractInfo>(out var info))
            {
                Inner_Execute(info);
            }
            else
            {
                Debug.Assert(false, "failed acquire contractInfo");
            }

            return this;
        }

        private void Inner_Execute(BaseContractInfo info)
        {
            foreach (BaseInteractionStateCallback callback in _callbacks)
            {
                var behaviour = info.GetBehaviourOrNull(callback.Type);

                if (behaviour != null)
                {
                    callback.Invoke(behaviour);
                }
            }
        }
        
    }
    public static class CollisionInteractionUtil
    {
        public static InteractionState CreateState()
        {
            var state = new InteractionState()
            {
            };

            return state;
        }
        public static SelectInteractionState CreateSelectState()
        {
            var state = new SelectInteractionState()
            {
            };

            return state;
        }

        public static bool OnCollision(Collider2D other, ICollisionInteraction interaction, bool isActivating, out CollisionInteraction result)
        {
            result = null;
            
            if (interaction.DetectedOnly) return false;
            if (!interaction.IsEnabled) return false;
            
            int layer = 1 << other.gameObject.layer;
            if ((layer & interaction.TargetLayerMask.value) != layer) return false;
            
            if (other.gameObject.TryGetComponent<CollisionInteraction>(out var com))
            {
                if (!com.IsEnabled) return false;
                if (com.ListeningOnly) return false;

                result = com;

                if (isActivating)
                {
                    interaction.Activate(com.ContractInfo);
                }
                else
                {
                    interaction.DeActivate(com.ContractInfo);
                }
            }

            return true;
        }
    }
}