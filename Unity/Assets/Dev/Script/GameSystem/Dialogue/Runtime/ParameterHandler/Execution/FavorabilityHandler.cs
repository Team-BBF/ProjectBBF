using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/Favorability", fileName = "NewFavorability")]
    public class FavorabilityHandler : ParameterHandlerArgsT<int>
    {
        protected override object OnExecute(int arg0)
        {
            //TODO: 후에 호감도 증감 로직 구현
            Debug.Log($"호감도 +{arg0}!");

            return null;
        }
    }
}