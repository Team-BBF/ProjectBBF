using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DS.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DS.Runtime
{
    public abstract class ParameterNodeData : DialogueNodeData
    {
        [Serializable]
        public class Warp
        {
            public string Target;
            public int IntValue;
            public float FloatValue;
            public string StringValue;
            public Object ObjectValue;
            public bool BoolValue;
            
            public bool IsEqual(Warp other)
            {
                if (Target != other.Target) return false;
                if (IntValue != other.IntValue) return false;
                if (FloatValue != other.FloatValue) return false;
                if (StringValue != other.StringValue) return false;
                if (ObjectValue != other.ObjectValue) return false;
                if (BoolValue != other.BoolValue) return false;
                
                return true;
            }
        }
        
        public ParameterHandler Handler;
        public Warp[] Warps = new Warp[]{};

        protected object[] GetArgs() => Warps.Select(x =>
        {
            switch (x.Target)
            {
                case "Int":
                    return (object)x.IntValue;
                case "Float":
                    return x.FloatValue;
                case "String":
                    return x.StringValue;
                case "Object":
                    return x.ObjectValue;
                case "Bool":
                    return x.BoolValue;
                default:
                    return "ERROR";
            }
        }).ToArray();
            
        public override bool IsEqual(DialogueNodeData other)
        {
            if (!base.IsEqual(other))
                return false;

            var otherNode = other as ParameterNodeData;

            if (otherNode is null) return false;
            if (Handler != otherNode.Handler) return false;
            if (Warps.Length != otherNode.Warps.Length) return false;

            for (int i = 0; i < Warps.Length; i++)
            {
                if (Warps[i].IsEqual(otherNode.Warps[i]) == false) return false;
            }
            
            return true;
        }
    }
}