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
    public abstract class PlayerInputQuestUI : BaseInput<PlayerController>
    {
        private bool _isTriggered;
        protected void ResetTriggerOnceFade()
        {
            _isTriggered = false;
        }
        public virtual void TriggerOnceResetFade()
        {
            if (_isTriggered) return;
            _isTriggered = true;
        }
    }

    public abstract class BasePlayerInputController : BaseInputController<PlayerController, PlayerInputFactory>
    {
        public override void InActivateInput()
        {
            UI.Value = null;
            Move.Value = null;
            Interact.Value = null;
            Tool.Value = null;
            QuestUI.Value = null;
        }

        public InputBinder<PlayerController, PlayerInputUI> UI { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputMove> Move { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputInteract> Interact { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputTool> Tool { get; private set; } = new();
        public InputBinder<PlayerController, PlayerInputQuestUI> QuestUI { get; private set; } = new();
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
        public abstract PlayerInputQuestUI CreateQuestUI();
    }

    public class DefaultBasePlayerInputController : BasePlayerInputController
    {
        public override void BindInput(PlayerInputFactory factory)
        {
            Move.Value = factory?.CreateMove();
            UI.Value = factory?.CreateUI();
            Interact.Value = factory?.CreateInteract();
            Tool.Value = factory?.CreateTool();
            QuestUI.Value = factory?.CreateQuestUI();
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
            QuestUI.Value = null;
        }

        public override void Update()
        {
            if (Tool.Value?.IsUsingTool is false &&
                Owner.Interactor.IsInteracting is false &&
                Owner.Dialogue.IsTalking is false &&
                Owner.PannelView.ViewState == PlayerPannelView.ViewType.Close &&
                Owner.RecipeBookPresenter.PreviewView.Visible is false)
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

            if (Owner.PannelView.ViewState == PlayerPannelView.ViewType.Close &&
                Owner.RecipeBookPresenter.PreviewView.Visible is false)
            {
                QuestUI.Value?.Update();
            }
            else
            {
                QuestUI.Value?.TriggerOnceResetFade();
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

        public override PlayerInputQuestUI CreateQuestUI()
        {
            return new PIQUDefault().Init(Owner).As<PlayerInputQuestUI>();
        }
    }
}