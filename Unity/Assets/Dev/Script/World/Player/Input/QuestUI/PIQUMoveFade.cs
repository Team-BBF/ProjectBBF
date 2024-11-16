using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBBF.Input
{
    public class PIQUMoveFade :  PIQUDefault
    {
        private InputAction _keyAction;
        private bool _keyFlag;
        public override void OnInit()
        {
            _keyAction = InputManager.Map.UI.FocusQuestMarker;
        }

        public override void Update()
        {
            if ((_keyAction.ReadValue<Vector2>().sqrMagnitude > 0f || Owner.MoveStrategy.Velocity.sqrMagnitude > 0f) && _keyFlag is false)
            {
                FadeoutObstacleUI();
                FadeoutIndicatorUI();
                _keyFlag = true;
            }
            else if((_keyAction.ReadValue<Vector2>().sqrMagnitude <= 0f || Owner.MoveStrategy.Velocity.sqrMagnitude <= 0f) && _keyFlag)
            {
                FadeinObstacleUI();
                FadeinIndicatorUI();
                _keyFlag = false;
            }
        }
    }
}