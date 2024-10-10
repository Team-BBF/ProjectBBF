using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


[UnitTitle("DialogueContext")]
[UnitCategory("Time")]
public class DialogueContextUnit : WaitUnit
{
    [DoNotSerialize] [PortLabelHidden] public ValueInput Context { get; private set; }

    protected override void Definition()
    {
        base.Definition();

        Context = ValueInput<DialogueContext>("context");
    }

    protected override IEnumerator Await(Flow flow)
    {
        var context = flow.GetValue<DialogueContext>(Context);

        while (true)
        {
            if (context.CanNext == false)
            {
                if (InputManager.Map.UI.DialogueSkip.triggered)
                {
                    yield return exit;
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            var task = context.Next();

            if (task.Status == UniTaskStatus.Faulted)
            {
                Debug.LogException(task.AsTask().Exception);
            }


            yield return new WaitUntil(() =>
                InputManager.Map.UI.DialogueSkip.triggered|| task.Status != UniTaskStatus.Pending);

            if (task.Status != UniTaskStatus.Pending)
            {
                yield return new WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered);
            }
        }
    }
}
