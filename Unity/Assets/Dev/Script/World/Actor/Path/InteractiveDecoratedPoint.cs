using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBBF.Event;
using UnityEngine;

public class InteractiveDecoratedPoint : MonoBehaviour
{
    [SerializeField] private Vector2 _interactingPivot;
    [SerializeField] private float _waitDuration;
    [SerializeField] private bool _teleport;
    [SerializeField] private bool _visitAndHide;

    public Vector2 InteractingPosition
    {
        get => _interactingPivot;
        set => _interactingPivot = value;
    }

    public float WaitDuration => _waitDuration;
    public bool Teleport => _teleport;
    public bool VisitAndHide => _visitAndHide;
}