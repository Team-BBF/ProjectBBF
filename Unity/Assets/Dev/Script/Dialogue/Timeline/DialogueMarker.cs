using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DS.Core;
using DS.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueMarker : Marker, INotification
{
    public PropertyName id => "Dialogue";

    [SerializeField] private DialogueContainer _container;


    public async UniTask OnPlay(PlayableDirector director, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_container == false)
            {
                Debug.LogWarning("Dialogue Marker not found");
                return;
            }


            var inst = DialogueController.Instance;
            DialogueContext ctx = inst.CreateContext(_container);
            inst.Visible = true;
            director.Pause();

            await ctx.Next();

            while (ctx.CanNext)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                if (InputManager.Map.UI.DialogueSkip.triggered)
                {
                    await ctx.Next();
                }
            }

            await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                cancellationToken);

            inst.Visible = false;
            director.Resume();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
    }
}
