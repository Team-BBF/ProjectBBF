using System;
using System.Collections;
using System.Collections.Generic;
using DS.Core;
using UnityEngine;


public abstract class DialogueItem
{
    public string ActorKey { get; private set; }
    public string PortraitKey { get; private set; }

    protected DialogueItem(string actorKey, string portraitKey)
    {
        ActorKey = actorKey;
        PortraitKey = portraitKey;
    }
}

public abstract class DialogueItemT<T> : DialogueItem where T : DialogueRuntimeNode
{
    public readonly T Node;
    protected DialogueItemT(T node, string actorKey, string portraitKey)
        : base(actorKey, portraitKey)
    {
        Node = node;
    }
}