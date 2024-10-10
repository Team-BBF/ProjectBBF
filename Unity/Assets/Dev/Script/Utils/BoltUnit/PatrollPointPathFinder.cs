using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class PatrollPointPathFinder : StateBehaviour
{
    private int? _currentIndex;
    private PatrolPointPath _before;


    public void Reduce()
    {
        if (_currentIndex.HasValue)
        {
            _currentIndex = Mathf.Max(_currentIndex.Value - 1, 0);
        }
    }

    [CanBeNull]
    public PatrolPoint GetNextPoint(PatrolPointPath path)
    {
        if (path is null) return null;
        if (_before != path)
        {
            _currentIndex = null;
            _before = path;
        }
        
        if (_currentIndex == null)
        {
            if (path.PatrollPoints.Any())
            {
                _currentIndex = 0;
                return path.PatrollPoints[_currentIndex.Value];
            }

            return null;
        }
        else
        {
            _currentIndex++;
            if (path.PatrollPoints.Count <= _currentIndex)
            {
                _currentIndex = 0;
            }

            return path.PatrollPoints[_currentIndex.Value];
        }
    }
}

[UnitCategory("ProjectBBF")]
[UnitTitle("PatrollPointPathFinder")]
public class PatrolPointPathFinderUnit : Unit
{
    private ValueInput _vPatrollPointPath;
    private ValueOutput _vPoint;

    private ControlInput _cStart;
    private ControlInput _cReduce;
    private ControlOutput _cOutput;

    private PatrolPoint _lastPoint;

    private ControlOutput OnExecute(Flow flow)
    {
        _lastPoint = BehaviourBinder.GetBinder(flow)
            .GetBehaviour<PatrollPointPathFinder>()
            .GetNextPoint(
                flow.GetValue<PatrolPointPath>(_vPatrollPointPath)
            );
        
        if (_lastPoint is null)
        {
            return null;
        }
        
        return _cOutput;
    }

    private ControlOutput OnReduce(Flow flow)
    {
        BehaviourBinder.GetBinder(flow)
            .GetBehaviour<PatrollPointPathFinder>()
            .Reduce()
            ;

        return null;
    }

    protected override void Definition()
    {
        _vPatrollPointPath = ValueInput<PatrolPointPath>("Patrol point path");
        _vPoint = ValueOutput("Patrol point", x => _lastPoint);

        _cOutput = ControlOutput("");
        _cStart = ControlInput("OnEnterState", OnExecute);
        _cReduce = ControlInput("Reduce", OnReduce);
    }
}