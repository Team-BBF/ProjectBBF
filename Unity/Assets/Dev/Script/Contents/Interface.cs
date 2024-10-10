using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DS.Core;
using ProjectBBF.Persistence;
using UnityEngine;

public interface IMinigameEventSignal
{
    public event Action OnGameInitEvent;
    public event Action OnGameEndEvent;
    public event Action OnPreGameEndEvent;
}

public abstract class MinigameBase<T> : MonoBehaviour, IMinigameEventSignal
    where T : MinigameData
{
    [SerializeField] private T _data;
    
    [SerializeField] private Transform _playerStartPoint;
    [SerializeField] private Transform _playerEndPoint;

    public T Data => _data;
    
    protected PlayerController Player { get; private set; }
    private bool _isRequestEnd;
    private MinigamePersistenceObject _persistenceObject;
    
    public event Action OnGameInitEvent;
    public event Action OnGameEndEvent;
    public event Action OnPreGameEndEvent;
    
    protected virtual void Awake()
    {
        MinigameController.Instance.OnSignalMinigameStart += OnStart;
        MinigameController.Instance.OnSignalMinigameEnd += OnEnd;

        _persistenceObject = PersistenceManager.Instance.LoadOrCreate<MinigamePersistenceObject>(Data.MinigameKey);

        if (Data.PlayCount == 0)
        {
            _persistenceObject.CanPlay = false;
        }
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
        
        _persistenceObject.IsPlaying = true;

        _ = OnSignalAsync();
    }

    private void OnEnd(string obj)
    {
        if (_data.MinigameKey != obj) return;

        RequestEndGame();
    }

    public void RequestEndGame()
    {
        _persistenceObject.IsPlaying = false;
        _isRequestEnd = true;
    }

    protected void Release()
    {
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
            OnGameInitEvent?.Invoke();
            await SceneLoader.Instance.WorkDirectorAsync(true, Data.DirectorKey);

            await OnTutorial();
            Player.StateHandler.TranslateState("EndOfDialogue");

            if (_isRequestEnd is false)
            {
                OnGameBegin();
            }

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
            OnPreGameEnd(_isRequestEnd);
            OnPreGameEndEvent?.Invoke();
            await SceneLoader.Instance.WorkDirectorAsync(true, Data.DirectorKey);
            Player.StateHandler.TranslateState("EndOfDialogue");
            
            await OnGameEnd(_isRequestEnd);
            OnGameEndEvent?.Invoke();

            if (_isRequestEnd)
            {
                await RunDialogue(Data.DialogueAfterGameEnd);
            }
            else
            {
                _persistenceObject.PlayCount++;
                if (Data.PlayCount > -1 && _persistenceObject.PlayCount >= Data.PlayCount)
                {
                    _persistenceObject.CanPlay = false;
                }
                await RunDialogue(Data.DialogueAfterGameExit);
            }
            
            OnGameRelease();

            Release();
            
            _persistenceObject.IsPlaying = false;
        }
        catch (Exception e)when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
            throw;
        }
       
    }

    protected UniTask RunDialogue(DialogueContainer container)
    {
        if (container == false) return UniTask.CompletedTask;
        
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
    protected abstract UniTask OnGameEnd(bool isRequestEnd);

    protected virtual void OnPreGameEnd(bool isRequestEnd)
    {
        
    }

}

// sample -> [CreateAssetMenu(menuName = "ProjectBBF/Data/Minigame/Farm", fileName = "FarmMinigameData")]
public abstract class MinigameData : ScriptableObject
{
    [SerializeField] private string _minigameKey;
    [SerializeField] private string _directorKey;
    [SerializeField] private int _playCount = 1;

    [SerializeField, Header("게임 종료 대사")] private DialogueContainer _dialogueAfterGameEnd;
    [SerializeField, Header("게임 중도 종료 대사")] private DialogueContainer _dialogueAfterGameExit;
    public string MinigameKey => _minigameKey;
    public string DirectorKey => _directorKey;

    public int PlayCount => _playCount;

    public DialogueContainer DialogueAfterGameEnd => _dialogueAfterGameEnd;
    public DialogueContainer DialogueAfterGameExit => _dialogueAfterGameExit;
}