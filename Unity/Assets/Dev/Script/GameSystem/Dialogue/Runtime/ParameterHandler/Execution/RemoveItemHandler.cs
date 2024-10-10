using System;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/Remove Item", fileName = "New Remove item")]
    public class RemoveItemHandler : ParameterHandlerArgsT<ItemData, int>
    {
        protected override object OnExecute(ItemData arg0, int arg1)
        {
            if (arg0 == false) return false;
            if (arg1 == 0) return false;

            var blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");

            bool flag = false;
            for (int i = 0; i < arg1; i++)
            {
                flag |= blackboard.Inventory.Model.PopItem(arg0);
            }

            if (flag)
            {
                blackboard.Inventory.Model.ApplyChanged();
            }

            return null;
        }
    }
}