using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/Minigame", fileName = "New Minigame")]
    public class MinigameHandler : ParameterHandlerArgsT<bool, string>
    {
        protected override object OnExecute(bool arg0, string arg1)
        {
            if (arg0)
            {
                MinigameController.Instance.StartMinigame(arg1);
            }
            else
            {
                MinigameController.Instance.EndMinigame(arg1);
            }
            
            return null;
        }
    }

}