using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("WaitUniTask")]
[UnitCategory("Time")]
public class WaitTask : Unit
{
    [field: DoNotSerialize, PortLabelHidden]
    public ControlInput Enter { get; private set; }
    
    [field: DoNotSerialize, PortLabelHidden]
    public ControlOutput Succeeded { get; private set; }
    [field: DoNotSerialize, PortLabelHidden]
    public ControlOutput Faulted { get; private set; }
    [field: DoNotSerialize, PortLabelHidden]
    public ControlOutput Canceled { get; private set; }
    
    [field: DoNotSerialize, PortLabelHidden]
    public ValueInput TargetTask { get; private set; } 
    
    protected override void Definition()
    {
        Enter = ControlInputCoroutine(nameof(Enter), Await);

        TargetTask = ValueInput<UniTask>("Task");

        Succeeded = ControlOutput("Succeeded");
        Faulted = ControlOutput("Faulted");
        Canceled = ControlOutput("Canceled");
    }

    private IEnumerator Await(Flow flow)
    {
        var task = flow.GetValue<UniTask>(TargetTask);
        yield return new WaitUntil(() => task.Status != UniTaskStatus.Pending);

        switch (task.Status)
        {
            case UniTaskStatus.Pending:
                throw new Exception();
            case UniTaskStatus.Succeeded:
                yield return Succeeded;
                break;
            case UniTaskStatus.Faulted:
                Debug.LogException(task.AsTask().Exception);
                yield return Faulted;
                break;
            case UniTaskStatus.Canceled:
                yield return Canceled;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
