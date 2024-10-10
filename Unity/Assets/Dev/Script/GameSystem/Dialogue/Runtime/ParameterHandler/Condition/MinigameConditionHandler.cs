using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/Minigame", fileName = "New Minigame condition")]
    public class MinigameConditionHandler : ParameterHandlerArgsT<string>
    {
        protected override object OnExecute(string key)
        {
            Debug.Assert(string.IsNullOrEmpty(key) is false);

            return MinigameController.Instance.CurrentGameKey == key;
        }
    }
}