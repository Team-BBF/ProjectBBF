using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/TimelineAsset", fileName = "New TimelineAsset")]
    public class TimelineAssetHandler : ParameterHandlerArgsT<TimelineAsset>
    {
        public static TimelineAsset TimelineAsset { get; set; }

        [RuntimeInitializeOnLoadMethod]
        private static void OnInit()
        {
            TimelineAsset = null;
        }

        protected override object OnExecute(TimelineAsset arg0)
        {
            if (arg0 == false) return null;

            TimelineAsset = arg0;

            return null;
        }
    }
}