using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DS.Core;
using JetBrains.Annotations;
using UnityEngine;

namespace DS.Core
{
    public abstract class DialogueRuntimeNode
    {

        public abstract bool IsLeaf { get; }

        /// <summary>
        /// 출력할 대사가 없으면 null을 반환.
        /// </summary>
        /// <returns></returns>
        [CanBeNull] public abstract DialogueItem CreateItem();
    }
}