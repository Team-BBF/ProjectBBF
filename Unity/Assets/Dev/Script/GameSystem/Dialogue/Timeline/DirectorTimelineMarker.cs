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

public class DirectorTimelineMarker : Marker, INotification
{
    public PropertyName id => "Director";

    [SerializeField] private bool _fadein = true;
    [SerializeField] private string _directorKey;
    

    public async UniTask OnPlay(PlayableDirector director)
    {
        try
        {
            director.Pause();
            await SceneLoader.Instance.WorkDirectorAsync(_fadein, string.IsNullOrEmpty(_directorKey) ? null : _directorKey);
            director.Resume();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
    }
}