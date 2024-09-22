using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Condition handler/Test", fileName = "New Test")]
    public class TestConditionHandler : ParameterHandlerArgsT<int>
    {
        protected override object OnExecute(int arg0)
        {
            Debug.Log(arg0 == 1);

            return arg0 == 1;
        }
    }
}