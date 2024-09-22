using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IBObjectDecoratedPoint : IObjectBehaviour
{
    public UniTask<IBObjectDecoratedPoint> Interact(CancellationToken token);
}