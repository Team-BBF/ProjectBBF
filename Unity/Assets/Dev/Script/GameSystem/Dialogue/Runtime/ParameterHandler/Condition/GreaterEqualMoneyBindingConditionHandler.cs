using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/GreaterEqualMoneyBinding", fileName = "New GreaterEqualMoneyBinding")]
    public class GreaterEqualMoneyBindingConditionHandler : ParameterHandlerArgsT<string>
    {
        protected override object OnExecute(string arg0)
        {
            if (string.IsNullOrWhiteSpace(arg0)) return false;
            
            var blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");

            if (ProcessorData.BindingTable.TryGetValue(arg0, out string data) is false)
            {
                Debug.LogError($"유효하지 않는 BindingKey({arg0})");
                return null;
            }

            int money = 0;

            try
            {
                money = Convert.ToInt32(data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            bool isGreateEqual = blackboard.Money >= money;

            return isGreateEqual;
        }
    }
}