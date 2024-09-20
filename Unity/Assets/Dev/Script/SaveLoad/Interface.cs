using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ProjectBBF.Persistence
{
    public abstract class DataTagAttribute : Attribute {}

    public class GameDataAttribute : DataTagAttribute {}
    public class UserDataAttribute : DataTagAttribute {}
}