using System;
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/Add Write Binding", fileName = "New Add Write Binding")]
    public class WriteBindingHandler : ParameterHandlerArgsT<string, string>
    {
        protected override object OnExecute(string arg0, string arg1)
        {
            if (string.IsNullOrWhiteSpace(arg0)) return null;
            if (string.IsNullOrWhiteSpace(arg1)) return null;

            arg0 = arg0.Trim();

            ProcessorData.BindingTable[arg0] = arg1;

            return null;
        }
    }
}