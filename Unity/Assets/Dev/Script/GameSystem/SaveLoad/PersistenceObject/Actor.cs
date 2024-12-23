using System.Collections.Generic;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [System.Serializable, GameData]
    public class ActorPersistenceObject
    {
        [DoNotEditable] public bool SavedPosition;
        public Vector3 LastPosition;

        public PatrolPointPath LastPath;
        public int PatrolPointIndex;
    }
}