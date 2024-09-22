using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("WaitUniTaskBool")]
[UnitCategory("Time")]
public class WaitTaskBool : Unit
{
    [field: DoNotSerialize, PortLabelHidden]
    public ControlInput Enter { get; private set; }
    
    [field: DoNotSerialize, PortLabelHidden]
    public ControlOutput Next { get; private set; }
    
    [field: DoNotSerialize, PortLabelHidden]
    public ValueInput TargetTask { get; private set; } 
    
    [field: DoNotSerialize, PortLabelHidden]
    public ValueOutput Result { get; private set; }

    private bool _result;
    
    protected override void Definition()
    {
        Enter = ControlInputCoroutine(nameof(Enter), Await);

        TargetTask = ValueInput<UniTask<bool>>("Task");
        Result = ValueOutput("Result", flow => _result);

        Next = ControlOutput("Next");
    }

    private IEnumerator Await(Flow flow)
    {
        var task = flow.GetValue<UniTask<bool>>(TargetTask);
        yield return new WaitUntil(() => task.Status != UniTaskStatus.Pending);

        switch (task.Status)
        {
            case UniTaskStatus.Pending:
                _result = false;
                Debug.Assert(false);
                break;
            case UniTaskStatus.Succeeded:
                _result = task.GetAwaiter().GetResult();
                break;
            case UniTaskStatus.Faulted:
                Debug.LogException(task.AsTask().Exception);
                _result = false;
                break;
            case UniTaskStatus.Canceled:
                _result = false;
                break;
            default:
                _result = false;
                Debug.Assert(false);
                break;
        }
        
        
        yield return Next;
    }
}