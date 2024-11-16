using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBBF.Input
{
    public class PIQUDefault :  PlayerInputQuestUI
    {
        private InputAction _keyAction;
        private bool _keyFlag;

        private bool _triggerFlag;
        public override void OnInit()
        {
            _keyAction = InputManager.Map.UI.FocusQuestMarker;
        }

        public override void Update()
        {
            ResetTriggerOnceFade();
            
            if (_keyAction.ReadValue<float>() > 0f && _keyFlag is false)
            {
                FadeoutObstacleUI();
                FadeoutIndicatorUI();
                _keyFlag = true;
            }
            else if(_keyAction.ReadValue<float>() <= 0f && _keyFlag)
            {
                FadeinObstacleUI();
                FadeinIndicatorUI();
                _keyFlag = false;
            }
        }
        
        protected void FadeoutObstacleUI()
        {
            if (QuestManager.Instance == false) return;
            
            foreach (QuestIndicatorObstacleUI obstacle in QuestManager.Instance.IndicatorObstacleList)
            {
                obstacle.DoFade(0.2f, 0.2f);
            }
        }

        protected void FadeinObstacleUI()
        {
            if (QuestManager.Instance == false) return;
            
            foreach (QuestIndicatorObstacleUI obstacle in QuestManager.Instance.IndicatorObstacleList)
            {
                obstacle.DoFade(1f, 0.2f);
            }
        }

        
        protected void FadeoutIndicatorUI()
        {
            if (QuestManager.Instance == false) return;
            
            foreach (QuestIndicatorUI obstacle in QuestManager.Instance.IndicatorList)
            {
                obstacle.BeginAnimateFocus();
            }
        }

        protected void FadeinIndicatorUI()
        {
            if (QuestManager.Instance == false) return;
            
            foreach (QuestIndicatorUI obstacle in QuestManager.Instance.IndicatorList)
            {
                obstacle.EndAnimateFocus();
            }
        }

        public override void Release()
        {
        }

        public override void TriggerOnceResetFade()
        {
            base.TriggerOnceResetFade();
            
            FadeinIndicatorUI();
            FadeinObstacleUI();
        }
    }
}