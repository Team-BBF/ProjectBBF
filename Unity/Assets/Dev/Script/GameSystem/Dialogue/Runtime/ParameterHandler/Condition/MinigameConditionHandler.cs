﻿using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/Minigame", fileName = "New Minigame condition")]
    public class MinigameConditionHandler : ParameterHandlerArgsT<string>
    {
        protected override object OnExecute(string key)
        {
            Debug.Assert(string.IsNullOrEmpty(key) is false);

            var data = PersistenceManager.Instance.GetCachedPersistenceObj(ref key) as MinigamePersistenceObject;
            if (data is null)
            {
                return false;
            }

            return data.IsPlaying;
        }
    }
}