using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectBBF.Persistence;
using UnityEngine;

public class MoveToWorld : MonoBehaviour
{
    [SerializeField] private SceneName _scene;
    [SerializeField] private string _directorKey = "BlackAlpha";
    [SerializeField] private bool _fadeOut;
    [SerializeField] private bool _fadeIn;
    [SerializeField] private bool _save;
    [SerializeField] private bool _load;
    [SerializeField] private Transform _initPlayerPosition;

    public void MoveWorld()
    {

        var obj = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(x => x.CompareTag("Player"));
        if (obj == false) return;
        if (obj.TryGetComponent(out PlayerController pc) is false) return;
        if (_initPlayerPosition == false) return;

        var pos = _initPlayerPosition.position;

        pc.Blackboard.CurrentPosition = pos;

        _ = UniTask.Create(async () =>
        {
            var loaderInst = SceneLoader.Instance;
            var PersistenceInst = PersistenceManager.Instance;
            pc.Blackboard.IsMoveStopped = true;
            pc.Blackboard.IsInteractionStopped = true;
            
            if (_fadeOut)
            {
                _ = await loaderInst.WorkDirectorAsync(false, _directorKey);
            }

            if (_save)
            {
                PersistenceInst.SaveGameDataCurrentFileName();
            }

            if (_load)
            {
                _ = await loaderInst.UnloadImmutableScenesAsync();
                
                PersistenceInst.LoadGameDataCurrentFileName();
                
                _ = await loaderInst.LoadImmutableScenesAsync();
            }
            else if (pc)
            {
                pc.transform.position = pos;
            }

            _ = await loaderInst.LoadWorldAsync(_scene);
            
            if (_fadeIn)
            {
                _ = await loaderInst.WorkDirectorAsync(true, _directorKey);
            }

            pc.Blackboard.IsMoveStopped = false;
            pc.Blackboard.IsInteractionStopped = false;
        });
    }
}