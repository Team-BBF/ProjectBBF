using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DS.Core;
using UnityEngine;

public abstract class MinigameBase<T> : MonoBehaviour where T : MinigameData
{
    [SerializeField] private T _data;
    
    [SerializeField] private Transform _playerStartPoint;
    [SerializeField] private Transform _playerEndPoint;

    [SerializeField] private bool _playOnce;


    public T Data => _data;
    
    protected PlayerController Player { get; private set; }
    private bool _isRequestEnd;
    
    protected virtual void Awake()
    {
        MinigameController.Instance.OnSignalMinigameStart += OnStart;
        MinigameController.Instance.OnSignalMinigameEnd += OnEnd;
    }

    protected virtual void OnDestroy()
    {
        if (MinigameController.Instance == false) return;
        
        MinigameController.Instance.OnSignalMinigameStart -= OnStart;
        MinigameController.Instance.OnSignalMinigameEnd -= OnEnd;
    }

    private void OnStart(string obj)
    {
        if (_data.MinigameKey != obj) return;

        if (Player == false)
        {
            foreach (var storedObject in GameObjectStorage.Instance.StoredObjects)
            {
                if (storedObject.CompareTag("Player") && storedObject.TryGetComponent(out PlayerController pc))
                {
                    Player = pc;
                    break;
                }
            }
        }
        
        MinigameController.Instance.CurrentGameKey = _data.MinigameKey;

        _ = OnSignalAsync();
    }

    private void OnEnd(string obj)
    {
        if (_data.MinigameKey != obj) return;

        if (MinigameController.Instance.CurrentGameKey == _data.MinigameKey)
        {
            MinigameController.Instance.CurrentGameKey = null;
        }

        _isRequestEnd = true;
    }

    protected void Release()
    {
        if (MinigameController.Instance.CurrentGameKey == _data.MinigameKey)
        {
            MinigameController.Instance.CurrentGameKey = null;
        }
        
        _isRequestEnd = false;
    }

    private async UniTask OnSignalAsync()
    {
        try
        {
            
            DialogueController.Instance.ResetDialogue();
            Player.StateHandler.TranslateState("EndOfInteractionDialogue");
            Player.StateHandler.TranslateState("ToDialogue");
            await SceneLoader.Instance.WorkDirectorAsync(false, Data.DirectorKey);
            Player.transform.position = (Vector2)_playerStartPoint.position;
            OnGameInit();
            await SceneLoader.Instance.WorkDirectorAsync(true, Data.DirectorKey);
            Player.StateHandler.TranslateState("EndOfDialogue");

            await OnTutorial();

            OnGameBegin();

            while (IsGameEnd() is false && _isRequestEnd is false)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
            }

            if (_isRequestEnd)
            {
                Player.StateHandler.TranslateState("EndOfInteractionDialogue");
            }
            Player.StateHandler.TranslateState("ToDialogue");
            await SceneLoader.Instance.WorkDirectorAsync(false, Data.DirectorKey);
            Player.transform.position = (Vector2)_playerEndPoint.position;
            OnGameRelease();
            await SceneLoader.Instance.WorkDirectorAsync(true, Data.DirectorKey);
            Player.StateHandler.TranslateState("EndOfDialogue");

            if (_isRequestEnd)
            {
                await RunDialogue(Data.DialogueAfterGameEnd);
            }
            else
            {
                if (_playOnce)
                {
                    MinigameController.Instance.PlayOnceTable.Add(Data.MinigameKey);
                }
                await RunDialogue(Data.DialogueAfterGameExit);

                foreach (var rewardItemSet in Data.Rewards)
                {
                    Player.Inventory.Model.PushItem(rewardItemSet.Item, rewardItemSet.Count);
                }
            }

            Release();
        }
        catch (Exception e)when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
            throw;
        }
       
    }

    protected UniTask RunDialogue(DialogueContainer container)
    {
        DialogueController.Instance.ResetDialogue();
        Player.StateHandler.TranslateState("ToDialogue");
        return Player.Dialogue.RunDialogue(container).ContinueWith(_ =>
        {
            Player.StateHandler.TranslateState("EndOfDialogue");
        });
    }

    protected abstract void OnGameInit();
    protected abstract UniTask OnTutorial();
    protected abstract void OnGameBegin();
    protected abstract void OnGameRelease();
    protected abstract bool IsGameEnd();
    
}

// sample -> [CreateAssetMenu(menuName = "ProjectBBF/Data/Minigame/Farm", fileName = "FarmMinigameData")]
public abstract class MinigameData : ScriptableObject
{
    [SerializeField] private string _minigameKey;
    [SerializeField] private string _directorKey;
    
    [SerializeField] private List<ItemDataSerializedSet> _rewards;

    [SerializeField] private DialogueContainer _dialogueAfterGameEnd;
    [SerializeField] private DialogueContainer _dialogueAfterGameExit;
    public string MinigameKey => _minigameKey;
    public string DirectorKey => _directorKey;

    public DialogueContainer DialogueAfterGameEnd => _dialogueAfterGameEnd;
    public DialogueContainer DialogueAfterGameExit => _dialogueAfterGameExit;

    public IReadOnlyList<ItemDataSerializedSet> Rewards => _rewards;
}