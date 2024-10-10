using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/LoadWorld", fileName = "New LoadWorld")]
    public class LoadWorldHandler : ParameterHandlerArgsT<string, float, float>
    {
        protected override object OnExecute(string worldSceneName, float x, float y)
        {
            var loader = SceneLoader.Instance;

            _ = loader
                    .WorkDirectorAsync(false)
                    .ContinueWith(async _ =>
                    {
                        await loader.LoadWorldAsync(worldSceneName);
                        var obj = GameObjectStorage.Instance.StoredObjects.FirstOrDefault(gameObject => gameObject.CompareTag("Player"));
                        if (obj && obj.TryGetComponent(out PlayerController pc))
                        {
                            pc.transform.position = new Vector3(x, y, 0f);
                        }
                        
                    })
                    .ContinueWith(() => loader.WorkDirectorAsync(true))
                ;
            
            return null;
        }
    }

}