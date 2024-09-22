using System.Collections;
using System.Collections.Generic;
using MyBox.EditorTools;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

//[CustomEditor(typeof(GrownDefinition))]
public class GrownDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GrownDefinition obj = (GrownDefinition)target;
        
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_needGrowingToNextGrowingStep")); 
        
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(obj);
            serializedObject.ApplyModifiedProperties();
        }

    }
}
