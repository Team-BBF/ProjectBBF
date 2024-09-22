using System.Collections.Generic;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [System.Serializable]
    public class Favorability
    {
        public int CurrentFavorability;
        public List<string> ExecutedDialogueGuid;
    }
}