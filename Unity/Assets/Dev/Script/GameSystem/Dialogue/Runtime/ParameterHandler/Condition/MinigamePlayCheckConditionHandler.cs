using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/Minigame play checker", fileName = "New Minigame  check play condition")]
    public class MinigamePlayCheckConditionHandler : ParameterHandlerArgsT<string>
    {
        protected override object OnExecute(string key)
        {
            Debug.Assert(string.IsNullOrEmpty(key) is false);

            return MinigameController.Instance.PlayOnceTable.Contains(key);
        }
    }
}