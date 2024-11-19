using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Persistence;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/Ed Fail", fileName = "New Ed fail")]
    public class EdFailConditionHandler : ParameterHandlerArgsT
    {
        protected object OnExecute()
        {
            BakeryIngredientTableData table = Resources.Load<BakeryIngredientTableData>("Data/Dat_Bakery_Table_EdFail");

            if (table == false)
            {
                Debug.LogError("Fail table을 찾을 수 없음");
                return false;
            }

            if (ProcessorData.BindingTable.TryGetValue(BucketFavorability.BUCKET_ITEM_0_KEY_KEY, out string key) is
                false)
            {
                Debug.LogWarning($"버킷에서 아이템을 찾을 수 없음.");
                return false;
            }

            ItemData contains = table.Ingredients.FirstOrDefault(x => x.ItemKey == key);

            return (bool)contains;
        }
    }
}