using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/Minigame play checker", fileName = "New Minigame  check play condition")]
    public class MinigamePlayCheckConditionHandler : ParameterHandlerArgsT<string>
    {
        protected override object OnExecute(string key)
        {
            Debug.Assert(string.IsNullOrEmpty(key) is false);

            var data = PersistenceManager.Instance.GetCachedPersistenceObj(ref key) as MinigamePersistenceObject;
            if (data is null)
            {
                Debug.LogError($"등록되지 않은 미니게임({key})을 검사했습니다.");
                return false;
            }
            
            return data.CanPlay is false;
        }
    }
}