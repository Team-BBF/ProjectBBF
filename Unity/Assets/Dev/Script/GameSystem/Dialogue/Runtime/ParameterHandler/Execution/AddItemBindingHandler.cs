using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/Add Item Binding", fileName = "New Add item Binding")]
    public class AddItemBindingHandler : ParameterHandlerArgsT<string, int>
    {
        protected override object OnExecute(string arg0, int arg1)
        {
            if (string.IsNullOrWhiteSpace(arg0)) return null;
            if (arg1 == 0) return null;

            var blackboard = PersistenceManager.Instance.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");

            arg0 = arg0.Trim();

            if (ProcessorData.BindingTable.TryGetValue(arg0, out string key) is false)
            {
                Debug.LogError($"유효하지 않는 BindingKey({arg0})");
                return null;
            }
            
            AsyncOperationHandle<IList<ItemData>> itemHandle = Addressables.LoadAssetsAsync<ItemData>
            (
                new object[] { key },
                null,
                Addressables.MergeMode.Union
            );
            itemHandle.WaitForCompletion();

            var itemData = itemHandle.Result.FirstOrDefault();
            if (itemData == false)
            {
                Debug.LogError($"찾을 수 없는 BindingKey({arg0}), DataKey({key})");
                return null;
            }

            bool success = blackboard.Inventory.Model.PushItem(itemData, arg1) is 0;
            if (success)
            {
                blackboard.Inventory.Model.ApplyChanged();
            }

            return null;
        }
    }
}