using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MyBox;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.SceneManagement;

[Singleton(ESingletonType.Global)]
public class SceneLoader : MonoBehaviourSingleton<SceneLoader>
{
    private List<string> _loadedAddtiveScenes;
    public ImmutableSceneTable ImmutableSceneTable { get; private set; }

    public bool IsLoadedImmutableScenes { get; private set; }
    public bool IsProgress { get; private set; }
    public IReadOnlyList<string> LoadedAddtiveScenes => _loadedAddtiveScenes;

    public event Action<string> WorldPreLoaded;
    public event Action<string> WorldLoaded;
    public event Action<string> WorldPostLoaded;
    public event Action FadeoutComplete;
    public event Action FadeinComplete;

    public string DefaultDirectorKey { get; } = "BlackAlpha";

    private Dictionary<string, ScreenDirector> _directors;
    
    public string CurrentWorldScene { get; private set; }
    public override void PostInitialize()
    {
        ImmutableSceneTable = Resources.Load<ImmutableSceneTable>("Data/ImmutableSceneTable");
        var directorList = Resources.LoadAll<ScreenDirector>("Feature/ScreenDirector");
        
        _directors = new();
        directorList.ForEach(x =>
        {
            var com = GameObject.Instantiate(x, transform);
            com.Enabled = false;
            _directors[x.Key] = com;
        });
        
        _loadedAddtiveScenes = new(5);
    }

    public override void PostRelease()
    {
        _loadedAddtiveScenes = null;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.M))
    //    {
    //        LoadWorldAsync("World_DaffodilLake_Arakar").ContinueWith(_ => LoadImmutableScenesAsync()).Forget();
    //    }
    //}

    private async UniTask Internal_UnloadAllAdditiveSceneAsync()
    {
        List<UniTask> tasks = new List<UniTask>(_loadedAddtiveScenes.Count);
        
        foreach (string sceneName in _loadedAddtiveScenes)
        {
            if (ImmutableSceneTable.Scenes.Contains(sceneName)) continue;
            
            AsyncOperation oper = SceneManager.UnloadSceneAsync(sceneName);
            if (oper is null) continue;
            
            oper.allowSceneActivation = true;

            var task = oper
                .ToUniTask(null, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);
            
            tasks.Add(task);
        }
        
        await UniTask.WhenAll(tasks);
        _loadedAddtiveScenes.Clear();
    }

    public async UniTask<bool> UnloadAllMapAsync()
    {
        if (IsProgress) return false;
        IsProgress = true;
        
        List<UniTask> tasks = new List<UniTask>(_loadedAddtiveScenes.Count);
        
        foreach (string sceneName in _loadedAddtiveScenes)
        {
            if (ImmutableSceneTable.Scenes.Contains(sceneName)) continue;
            if (sceneName == CurrentWorldScene) continue;
            
            AsyncOperation oper = SceneManager.UnloadSceneAsync(sceneName);
            if (oper is null) continue;
            
            oper.allowSceneActivation = true;

            var task = oper
                .ToUniTask(null, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);
            
            tasks.Add(task);
        }
        
        await UniTask.WhenAll(tasks);
        _loadedAddtiveScenes.Clear();
        
        IsProgress = false;
        return true;
    }
    public async UniTask<bool> LoadAllMapAsync()
    {
        if (IsProgress) return false;
        IsProgress = true;
        
        var loader = GetRootLoader(SceneManager.GetSceneByName(CurrentWorldScene).GetRootGameObjects());
        var list = await loader.LoadAsync();

        foreach ((string, AsyncOperation) tuple in list)
        {
            _loadedAddtiveScenes.Add(tuple.Item1);
            tuple.Item2.allowSceneActivation = true;
        }
        
        IsProgress = false;
        return true;
    }

    public async UniTask<bool> LoadImmutableScenesAsync()
    {
        if (IsProgress) return false;
        if (IsLoadedImmutableScenes) return false;

        IsProgress = true;
        
        TryLoadEntryScene();
        
        List<UniTask> tasks = new List<UniTask>(ImmutableSceneTable.Scenes.Count);
        
        foreach (string sceneName in ImmutableSceneTable.Scenes)
        {
            AsyncOperation oper = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (oper is null) continue;
            oper.allowSceneActivation = true;

            var task = oper
                .ToUniTask(null, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);
            
            tasks.Add(task);
        }
        
        await UniTask.WhenAll(tasks);

        IsLoadedImmutableScenes = true;
        IsProgress = false;
        return true;
    }

    public async UniTask<bool> UnloadImmutableScenesAsync()
    {
        if (IsProgress) return false;
        if (IsLoadedImmutableScenes == false) return false;

        IsProgress = true;
        
        List<UniTask> tasks = new List<UniTask>(ImmutableSceneTable.Scenes.Count);
        
        foreach (string sceneName in ImmutableSceneTable.Scenes)
        {
            AsyncOperation oper = SceneManager.UnloadSceneAsync(sceneName);
            if (oper is null) continue;

            var task = oper
                .ToUniTask(null, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);
            
            tasks.Add(task);
        }
        
        await UniTask.WhenAll(tasks);

        IsLoadedImmutableScenes = false;
        IsProgress = false;
        return true;
    }

    public async UniTask<bool> WorkDirectorAsync(bool fadeIn, string key = null)
    {
        if (key is null)
        {
            key = DefaultDirectorKey;
        }
        if (_directors.TryGetValue(key, out var director) == false)
        {
            Debug.LogError($"SceneLoader에서 Director key({key})를 찾을 수 없습니다.");
            return false;
        }
        
        Debug.Assert(director is not null);

        if (fadeIn)
        {
            director.Enabled = true;
            await director.Fadein();
            director.Enabled = false;
            FadeinComplete?.Invoke();
        }
        else
        {
            director.Enabled = true;
            await director.Fadeout();
            FadeoutComplete?.Invoke();
        }

        return true;
    }
    
    public async UniTask<bool> LoadWorldAsync(string worldSceneName)
    {
        if (IsProgress) return false;
        IsProgress = true;

        try
        {
            if (ImmutableSceneTable.Scenes.Contains(worldSceneName))
            {
                Debug.LogError("Immutable scene은 single로 불러올 수 없습니다.");
                IsProgress = false;
                return false;
            }
            
            
            WorldPreLoaded?.Invoke(worldSceneName);

            TryLoadEntryScene();

            await Internal_UnloadAllAdditiveSceneAsync();

            var oper = SceneManager.LoadSceneAsync(worldSceneName, LoadSceneMode.Additive);

            if (oper is null)
            {
                IsProgress = false;
                Debug.LogError($"World scene({worldSceneName})을 불러오는데 실패했습니다.");
                return false;
            }
            
            oper.allowSceneActivation = true;
            await oper.ToUniTask(null, PlayerLoopTiming.Update, GlobalCancelation.PlayMode);

            _loadedAddtiveScenes.Add(worldSceneName);

            var loader = GetRootLoader(SceneManager.GetSceneByName(worldSceneName).GetRootGameObjects());

            if (loader == false)
            {
                Debug.LogError($"RootSceneLoader를 찾지 못했습니다. WorldSceneName({worldSceneName})");
                IsProgress = false;
                return false;
            }
            
            List<(string, AsyncOperation)> loadedScenes = await loader.LoadAsync();

            
            _loadedAddtiveScenes.AddRange(loadedScenes.Select(x=>x.Item1));

            loadedScenes.ForEach(x => x.Item2.allowSceneActivation = true);

            await UniTask.WhenAll(loadedScenes.Select(x =>
                x.Item2.ToUniTask(null, PlayerLoopTiming.Update, GlobalCancelation.PlayMode)));

            _ = Resources.UnloadUnusedAssets().WithCancellation(GlobalCancelation.PlayMode);

            WorldLoaded?.Invoke(worldSceneName);
            await UniTask.Yield();
            WorldPostLoaded?.Invoke(worldSceneName);

        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            Debug.LogException(e);
        }
        finally
        {
            IsProgress = false;
        }

        CurrentWorldScene = worldSceneName;

        return true;
    }

    private void TryLoadEntryScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if (currentSceneName == "EntryScene") return;
        SceneManager.LoadScene("EntryScene", LoadSceneMode.Single);
    }


    private readonly Queue<Transform> _objTemp = new(50);
    private RootSceneLoader GetRootLoader(GameObject[] objs)
    {
        foreach (var obj in objs)
        {
            var first = obj.GetComponent<RootSceneLoader>();
            if (first)
            {
                return first;
            }
            
            _objTemp.Clear();
            _objTemp.Enqueue(obj.transform);

            while (_objTemp.Any())
            {
                int length = _objTemp.Count;
                for (int i = 0; i < length; i++)
                {
                    var temp = _objTemp.Dequeue();
                    
                    for (int j = 0; j < temp.childCount; j++)
                    {
                        var com = temp.GetComponent<RootSceneLoader>();
                        if (com)
                        {
                            _objTemp.Clear();
                            return com;
                        }
                
                        _objTemp.Enqueue(temp.GetChild(i));
                    }
                }
            }
        }

        return null;
    }
}
