using MyBox;
using UnityEngine;

namespace ProjectBBF.Input
{
    public abstract class PlayerInputUI : BaseInput<PlayerController>
    {
    }

    public abstract class PlayerInputMove : BaseInput<PlayerController>
    {
        private bool _isTriggeredOnceRsetVelocity;
        protected void ResetTriggerOnceVelocity()
        {
            _isTriggeredOnceRsetVelocity = false;
        }
        public void TriggerOnceResetVelocity()
        {
            if (_isTriggeredOnceRsetVelocity) return;
            _isTriggeredOnceRsetVelocity = true;
            
            Owner.MoveStrategy.ResetVelocity();
        }
    }

    public abstract class PlayerInputInteract : BaseInput<PlayerController>
    {
    }

    public abstract class PlayerInputTool : BaseInput<PlayerController>
    {
        public bool IsUsingTool { get; protected set; }
    }

    public class InputBinder<TOwner, TInput>
        where TInput : BaseInput<TOwner>
    {
        private TInput _value;
        public TInput Value
        {
            get => _value;
            set
            {
                if (_value is not null)
                {
                    _value.Release();
                }

                _value = value;
            }
        }
    }

    public abstract class BasePlayerInputController : BaseInputController<PlayerController, PlayerInputFactory>
    {
        public InputBinder<PlayerController, PlayerInputUI> UI { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputMove> Move { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputInteract> Interact { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputTool> Tool { get; private set; } = new();
    }

    public abstract class PlayerInputFactory : IInputFactory<PlayerController>
    {
        public PlayerController Owner { get; private set; }
        public void Init(PlayerController owner)
        {
            Owner = owner;
        }

        public abstract PlayerInputMove CreateMove();

        public abstract PlayerInputUI CreateUI();

        public abstract PlayerInputInteract CreateInteract();

        public abstract PlayerInputTool CreateTool();
    }

    public class DefaultBasePlayerInputController : BasePlayerInputController
    {
        public override void BindInput(PlayerInputFactory factory)
        {
            Move.Value = factory?.CreateMove();
            UI.Value = factory?.CreateUI();
            Interact.Value = factory?.CreateInteract();
            Tool.Value = factory?.CreateTool();
        }
        public override void OnInit()
        {
            
        }

        public override void Release()
        {
            Move.Value = null;
            UI.Value = null;
            Interact.Value = null;
            Tool.Value = null;
        }

        public override void Update()
        {
            if (Tool.Value?.IsUsingTool is false &&
                Owner.Interactor.IsInteracting is false &&
                Owner.Dialogue.IsTalking is false &&
                Owner.PannelView.ViewState == PlayerPannelView.ViewType.Close)
            {
                Move.Value?.Update();
                Tool.Value?.Update();
            }
            else
            {
                Move.Value?.TriggerOnceResetVelocity();
            }

            if(Owner.Interactor.IsInteracting is false)
            {
                Interact.Value?.Update();
                UI.Value?.Update();
            }
        }
    }

    public class DefaultPlayerInputFactory : PlayerInputFactory
    {
        public override PlayerInputMove CreateMove()
        {
            return new PIMDefault().Init(Owner).As<PlayerInputMove>();
        }

        public override PlayerInputUI CreateUI()
        {
            return new PIUIDefault().Init(Owner).As<PlayerInputUI>();
        }

        public override PlayerInputInteract CreateInteract()
        {
            return new PIINDefault().Init(Owner).As<PlayerInputInteract>();
        }

        public override PlayerInputTool CreateTool()
        {
            return new PITODefault().Init(Owner).As<PlayerInputTool>();
        }
    }
}