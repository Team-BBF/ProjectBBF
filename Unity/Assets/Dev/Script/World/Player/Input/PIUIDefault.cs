using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectBBF.Input
{
    public class PIUIDefault : PlayerInputUI
    {
        public bool IsAnyUIVisible { get; private set; }

        private bool IsTriggeredUIAny =>
            InputManager.Map.UI.Inventory.triggered ||
            InputManager.Map.UI.Setting.triggered ||
            InputManager.Map.UI.RecipeBook.triggered ||
            InputManager.Map.UI.CloseUI.triggered;

        public override void OnInit()
        {
        }

        public override void Update()
        {
            if (IsAnyUIVisible is false)
            {
                if (InputManager.Map.UI.Inventory.triggered)
                {
                    _ = OnUIInventory();
                }

                if (InputManager.Map.UI.Setting.triggered)
                {
                    _ = OnUISetting();
                }

                if (InputManager.Map.UI.RecipeBook.triggered)
                {
                    _ = OnUIRecipe();
                }
            }
        }

        public override void Release()
        {
        }

        private async UniTask OnUIInventory()
        {
            bool canceled = false;

            canceled = await UniTask
                .NextFrame(PlayerLoopTiming.PostLateUpdate, Owner.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (canceled) return;

            Owner.PannelView.ViewState = PlayerPannelView.ViewType.Inv;
            Owner.QuestPresenter.Visible = true;
            IsAnyUIVisible = true;

            canceled = await UniTask
                .WaitUntil(() => IsTriggeredUIAny, PlayerLoopTiming.PostLateUpdate,
                    Owner.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (canceled) return;

            Owner.PannelView.ViewState = PlayerPannelView.ViewType.Close;
            IsAnyUIVisible = false;
        }

        private async UniTask OnUISetting()
        {
            bool canceled = false;

            canceled = await UniTask
                .NextFrame(PlayerLoopTiming.PostLateUpdate, Owner.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (canceled) return;

            Owner.PannelView.ViewState = PlayerPannelView.ViewType.Setting;
            Owner.RecipeBookPresenter.PreviewSummaryView.Visible = false;
            Owner.QuestPresenter.Visible = false;
            IsAnyUIVisible = true;


            canceled = await UniTask
                .WaitUntil(() => IsTriggeredUIAny, PlayerLoopTiming.PostLateUpdate,
                    Owner.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (canceled) return;

            Owner.PannelView.ViewState = PlayerPannelView.ViewType.Close;
            if (Owner.RecipeBookPresenter.PreviewSummaryView.Data is not null)
                Owner.RecipeBookPresenter.PreviewSummaryView.Visible = true;
            Owner.QuestPresenter.Visible = true;
            IsAnyUIVisible = false;
        }

        private async UniTask OnUIRecipe()
        {
            bool canceled = false;

            canceled = await UniTask
                .NextFrame(PlayerLoopTiming.PostLateUpdate, Owner.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (canceled) return;

            Owner.PannelView.ViewState = PlayerPannelView.ViewType.Close;
            Owner.RecipeBookPresenter.ListView.Visible = true;
            Owner.RecipeBookPresenter.PreviewView.Visible = true;
            IsAnyUIVisible = true;


            canceled = await UniTask
                .WaitUntil(() => IsTriggeredUIAny, PlayerLoopTiming.PostLateUpdate,
                    Owner.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (canceled) return;

            Owner.RecipeBookPresenter.ListView.Visible = false;
            Owner.RecipeBookPresenter.PreviewView.Visible = false;
            IsAnyUIVisible = false;
        }
    }
}