
using UnityEngine;

namespace ProjectBBF.Input
{
    public class PIMDefault :  PlayerInputMove
    {
        public override void OnInit()
        {
        }

        public override void Update()
        {
            Vector2 input = InputManager.Map.Player.Movement.ReadValue<Vector2>();
            Owner.MoveStrategy.Move(input);
        }

        public override void Release()
        {
            Owner.MoveStrategy.ResetVelocity();
        }
    }
}