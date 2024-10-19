﻿using System.Collections.Generic;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    [System.Serializable, GameData]
    public class MinigamePersistenceObject
    {
        public int PlayCount;
        public bool CanPlay = true;
        public bool IsPlaying = false;
    }
}