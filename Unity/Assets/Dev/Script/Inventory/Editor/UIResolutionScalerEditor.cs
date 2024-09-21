using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UIResolutionScaler))]

public class UIResolutionScalerEditor : Editor
{
    private readonly string[] POP_UP =
    {
        "Scale With Screen Size",
        "Constant Pixel Size",
        "Constant Physics Size",
    };
    
    public override void OnInspectorGUI()
    {
        GUILayout.Label("스케일 분기 기준 해상도");
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("_standardResolution"));
        
        GUILayout.Space(20);
        GUILayout.Label("기준 해상도보다 높을 때");
        serializedObject.FindProperty("_upperCase").intValue = DrawUpper(serializedObject.FindProperty("_upperCase").intValue, "_upper");
        
        
        GUILayout.Space(20);
        
        GUILayout.Label("기준 해상도보다 낮을 때");
        serializedObject.FindProperty("_lowerCase").intValue = DrawUpper(serializedObject.FindProperty("_lowerCase").intValue, "_lower");

        serializedObject.ApplyModifiedProperties();
    }

    private int DrawUpper(int index, string prefix)
    {
        index = EditorGUILayout.Popup("UI Scale Mode", index, POP_UP);

        if (index == 0)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(prefix + "ReferenceResolution"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(prefix + "ScreenMatchMode"));
        }
        else if (index == 1)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(prefix + "ScaleFactor"));
        }
        else if (index == 2)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(prefix + "PhysicalUnit"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(prefix + "FallbackScreenDPI"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(prefix + "DefaultSpriteDPI"));
        }
        
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(prefix + "ReferencePixelsPerUnit"));
        return index;
    }
}
#endif