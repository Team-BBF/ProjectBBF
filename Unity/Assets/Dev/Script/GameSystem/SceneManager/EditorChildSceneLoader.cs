#if UNITY_EDITOR
 
using UnityEditor;
using UnityEngine;
 
[CustomEditor(typeof(RootSceneLoader))]
public class ChildSceneLoaderInspectorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
 
        var currentInspectorObject = (RootSceneLoader)target;
 
        if (GUILayout.Button("Reset scene setup from config..."))
        {
            currentInspectorObject.ResetSceneSetupToConfig();
        }
    }
}
 
#endif