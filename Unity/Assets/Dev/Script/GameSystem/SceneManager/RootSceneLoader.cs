using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using MyBox;

/// <summary>
/// Allows setting a scene as a root scene and setting its child scenes. To use this, drag this component on any object in a scene to make that scene a root scene. In the background, ChildSceneLoader will automatically manage this.
/// </summary>
public class RootSceneLoader : MonoBehaviour
{
    [SerializeField] private bool _loadImmutableScene = true;

    [field: SerializeField, HideInInspector]
    private List<string> _scenes;

    public async UniTask<List<(string, AsyncOperation)>> LoadAsync()
    {
#if UNITY_EDITOR
        ApplySceneName();
#endif
        if (_scenes == null) return new List<(string, AsyncOperation)>();

        List<UniTask> tasks = new List<UniTask>(_scenes.Count);
        List<(string, AsyncOperation)> loadedScenes = new List<(string, AsyncOperation)>(_scenes.Count);

        foreach (string sceneName in _scenes)
        {
            //if (sceneName.Contains("World")) continue;
            if (SceneManager.GetActiveScene().name == sceneName) continue;
            if (SceneLoader.Instance.ImmutableSceneTable.Scenes.Contains(sceneName)) continue;

            AsyncOperation oper = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            oper.allowSceneActivation = false;

            string ls = sceneName;

            var task = UniTask
                .WaitUntil(() => oper.progress >= 0.9f, PlayerLoopTiming.Update, GlobalCancelation.PlayMode)
                .ContinueWith(() => loadedScenes.Add((ls, oper)));

            tasks.Add(task);
        }

        await UniTask.WhenAll(tasks);

        return loadedScenes;
    }

    private void Update()
    {
        // DO NOT DELETE keep this so we can enable/disable this script... (used in ChildSceneLoader)

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (GameObjectStorage.Instance && GameObjectStorage.Instance.TryGetPlayerController(out var pc))
            {
                pc.transform.position = (Vector2)transform.position;
            }
        }
    }


#if UNITY_EDITOR
    [SerializeField] public List<SceneAsset> ChildScenesToLoadConfig;

    [MenuItem("Scene/AutoPlayModeOn", false)]
    private static void OnAutoPlayModeOn()
    {
        if (Application.isPlaying) return;

        EditorPrefs.SetBool("__AUTO_PLAY_MODE__", true);
    }

    [MenuItem("Scene/AutoPlayModeOff", false)]
    private static void OnAutoPlayModeOff()
    {
        if (Application.isPlaying) return;

        EditorPrefs.SetBool("__AUTO_PLAY_MODE__", false);
    }

    [MenuItem("Scene/AutoPlayModeOn", true)]
    private static bool ValidateOnAutoPlayModeOn()
    {
        if (Application.isPlaying) return false;

        return EditorPrefs.GetBool("__AUTO_PLAY_MODE__") is false;
    }

    [MenuItem("Scene/AutoPlayModeOff", true)]
    private static bool ValidateOnAutoPlayModeOff()
    {
        return EditorPrefs.GetBool("__AUTO_PLAY_MODE__");
    }

    private void Start()
    {
        // 에디터에서 playmode 진입했을 때, 씬이 플레이 가능 상태가 아니면, 플레이 가능으로 만들어주는 로직
        if (EditorPrefs.GetBool("__AUTO_PLAY_MODE__") && SceneLoader.Instance.IsLoadedImmutableScenes == false)
        {
            string worldSceneName = gameObject.scene.name;

            _ = UniTask.Create(async () =>
            {
                if (_loadImmutableScene)
                {
                    await SceneLoader.Instance.LoadImmutableScenesAsync();
                }
                
                
                await SceneLoader.Instance.LoadWorldAsync(worldSceneName);
                
                SceneManager.GetSceneByName(worldSceneName).GetRootGameObjects().ForEach(y =>
                {
                    var root = y.GetComponent<RootSceneLoader>();

                    if (root == false)
                    {
                        root = y.GetComponentInChildren<RootSceneLoader>();
                    }

                    if (root)
                    {
                        var t = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(z =>
                            z.GetComponent<PlayerController>());

                        if (t.TryGetComponent(out PlayerController pc))
                        {
                            t.transform.SetXY(root.transform.position);
                            pc.Blackboard.CurrentWorld = worldSceneName;
                            pc.Blackboard.CurrentWorld = worldSceneName; // prev 때문에 두번
                        }
                    }
                });
                
            });
        }
    }
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        ApplySceneName();
    }

    private void ApplySceneName()
    {
        if (ChildScenesToLoadConfig is null) return;

        _scenes = new List<string>();

        ChildScenesToLoadConfig.ForEach(x =>
        {
            var split = x.name.Split("_");

            if (split.Length > 0 && split[0] == "World")
            {
                return;
            }

            _scenes.Add(x.name);
        });
    }

    public void ResetSceneSetupToConfig()
    {
        var sceneAssetsToLoad = ChildScenesToLoadConfig;

        if (ChildScenesToLoadConfig.FirstOrDefault(x => x.name == EditorSceneManager.GetActiveScene().name) is null)
        {
            return;
        }

        List<SceneSetup> sceneSetupToLoad = new List<SceneSetup>();
        foreach (var sceneAsset in sceneAssetsToLoad)
        {
            sceneSetupToLoad.Add(new SceneSetup()
                { path = AssetDatabase.GetAssetPath(sceneAsset), isActive = false, isLoaded = true });
        }

        sceneSetupToLoad[0].isActive = true;
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.RestoreSceneManagerSetup(sceneSetupToLoad.ToArray());

        ApplySceneName();
    }
#endif
}

#if UNITY_EDITOR


[InitializeOnLoad]
public class ChildSceneLoader
{
    static ChildSceneLoader()
    {
        EditorSceneManager.sceneOpened += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene _, OpenSceneMode mode)
    {
        if (mode != OpenSceneMode.Single || BuildPipeline.isBuildingPlayer)
            return; // try to load child scenes only for root scenes or if not building

        var scenesToLoadObjects = GameObject.FindObjectsOfType<RootSceneLoader>();
        if (scenesToLoadObjects.Length > 1)
        {
            throw new Exception("Should only have one root scene at once loaded");
        }

        if (scenesToLoadObjects.Length == 0 ||
            !scenesToLoadObjects[0].enabled) // only when we have a config and when that config is enabled
        {
            return;
        }

        scenesToLoadObjects[0].ResetSceneSetupToConfig();

        Debug.Log("Setup done for root scene and child scenes");
    }
}
#endif