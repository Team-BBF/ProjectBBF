using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DS.Runtime;
using Mobsoft.PixelStyleWaterShader;
using MyBox;
using ProjectBBF.Persistence;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogueCutSceneController : MonoBehaviour, INotificationReceiver
{
    [SerializeField] private bool _rollbackScene;
    [SerializeField] private bool _tryImmutableSceneLoadStopped;
    
    [SerializeField] private string _fadeinDirectorKey;
    [SerializeField] private string _fadeoutDirectorKey;
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private ESOContestResultPush _esoResultPush;
    [SerializeField] private ContestResultUI _resultUI;

    private const string FIRST_ACTOR_KEY = "Ahir";
    private const string SECOND_ACTOR_KEY = "Elisia";

    private const string CONTEST_RESULT_BINDING_KEY = "contestResultItem";
    
    private ItemData _lastResultItem;
    private ProcessorData _processorData;

    private void Awake()
    {
        _director.stopped += OnStopped;
        if (_esoResultPush)
        {
            _esoResultPush.OnEventRaised += OnResultItemPush;
        }

        _processorData = new ProcessorData(new Dictionary<string, string>(new List<KeyValuePair<string, string>>()
        {
            new("player", PersistenceManager.Instance.CurrentMetadata.PlayerName),
            new(CONTEST_RESULT_BINDING_KEY, "None"),
        }));
    }

    private void Start()
    {
        var po = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(x => x.CompareTag("Player"));
        if (po && po.TryGetComponent(out PlayerController pc))
        {
            pc.HudController.Visible = false;
            pc.Inventory.QuickInvVisible = false;
            pc.Blackboard.IsInteractionStopped = true;
            pc.QuestPresenter.Visible = false;
        }

    }

    private void OnDestroy()
    {
        if (GameObjectStorage.Instance == false) return;
        
        var po = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(x => x.CompareTag("Player"));
        if (po && po.TryGetComponent(out PlayerController pc))
        {
            pc.HudController.Visible = true;
            pc.Inventory.QuickInvVisible = true;
            pc.Blackboard.IsInteractionStopped = false;
            pc.QuestPresenter.Visible = true;
        }
        
        _director.stopped -= OnStopped;
        if (_esoResultPush)
        {
            _esoResultPush.OnEventRaised -= OnResultItemPush;
        }

        if (DialogueController.Instance)
        {
            DialogueController.Instance.ResetDialogue();
        }
        
    }

    private void OnStopped(PlayableDirector obj)
    {
        var loaderInst = SceneLoader.Instance;
        var persistenceInst = PersistenceManager.Instance;

        if (loaderInst == false || persistenceInst == false) return;


        if (_rollbackScene)
        {
            var blackboard = persistenceInst.LoadOrCreate<PlayerBlackboard>("Player_Blackboard");
            string scene = blackboard.CurrentWorld;
            
        
            _ = loaderInst
                    .WorkDirectorAsync(false, _fadeoutDirectorKey)
                    .ContinueWith(async _ =>
                    {
                        if (_tryImmutableSceneLoadStopped && loaderInst.IsLoadedImmutableScenes is false)
                        {
                            return await loaderInst.LoadImmutableScenesAsync();
                        }

                        return true;
                    })
                    .ContinueWith(_ => loaderInst.LoadWorldAsync(scene))
                    .ContinueWith(_ => loaderInst.WorkDirectorAsync(true, _fadeinDirectorKey))
                    .ContinueWith(_ => TimeManager.Instance.Resume())
                ;
        }
    }

    private void OnResultItemPush(ContestResultEvent obj)
    {
        if (obj.TargetItem)
        {
            _lastResultItem = obj.TargetItem;
            _processorData.BindingTable[CONTEST_RESULT_BINDING_KEY] = _lastResultItem.ItemName;
        }
    }


    public void Play()
    {
        _director.Play();
    }

    public void Stop()
    {
        _director.Stop();
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is DialogueMarker dialogueMarker)
        {
            _ = dialogueMarker.OnPlay(_director, _processorData, this.GetCancellationTokenOnDestroy());
        }
        else if (notification is BranchTimelineMarker)
        {
            _director.stopped -= OnStopped;
            var timelineAsset = TimelineAssetHandler.TimelineAsset;

            if (timelineAsset == false)
            {
                _director.Stop();
                Debug.LogError("분기된 타임라인이 없습니다.");
                return;
            }
            
            _director.playableAsset = timelineAsset;
            TimelineAssetHandler.TimelineAsset = null;
            _director.time = 0;
            _director.Play();
            
            _director.stopped += OnStopped;
        }
        else if (notification is DirectorTimelineMarker directorMarker)
        {
            _ = directorMarker.OnPlay(_director);
        }
        else if (notification is ContestResultMarker contestResultMarker)
        {
            if (_resultUI == false) return;
            
            _resultUI.Visible = true;
            TimelineAssetHandler.TimelineAsset = contestResultMarker.FailTimeline;
            if (_lastResultItem == false)
            {
                _lastResultItem = AhirContestResultBox.ResultItem;
                if (_lastResultItem == false)
                {
                    Debug.LogWarning("유효하지 않은 Result Item");
                }
            }
            
            List<ContestResultData.Record> results = new List<ContestResultData.Record>();

            ContestResultResolver.Instance.Resolve(contestResultMarker.Chapter, _lastResultItem, ref results);
                
            int firstIndex = results.FirstIndex(x => x.ActorKey is FIRST_ACTOR_KEY);
            int secondIndex = results.FirstIndex(x => x.ActorKey is SECOND_ACTOR_KEY);
            
            Sprite itemSpr = _lastResultItem ? _lastResultItem.ItemSprite : null;
            string itemName = _lastResultItem ? _lastResultItem.ItemName : "";

            if (firstIndex != -1 && secondIndex != -1)
            {
                // 둘 중 하나만 성공이라면 성공처리
                if (results[firstIndex].Failue is false && results[secondIndex].Failue is false)
                {
                    TimelineAssetHandler.TimelineAsset = contestResultMarker.SuccessTimeline;
                }
                _resultUI.Set(itemSpr, itemName, results[firstIndex].Text, results[secondIndex].Text);
            }
            else
            {
                Debug.LogError("유효하지 않은 Actor Key를 Resolve 했음");
            }
            
            _director.Pause();

            _ = UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                this.GetCancellationTokenOnDestroy())
                .ContinueWith(() =>
                {
                    _resultUI.Visible = false;
                    _director.Resume();
                });
        }
        else if (notification is MoveToSceneMarker moveToSceneMarker)
        {
            if (moveToSceneMarker.ESO)
            {
                _director.Stop();
                moveToSceneMarker.ESO.Raise();
            }
        }

    }
}