using System;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(SceneName))]
public class SceneNamePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // SceneName 객체에서 _sceneAsset과 _sceneName 필드 참조
        SerializedProperty sceneAssetProp = property.FindPropertyRelative("_sceneAsset");
        SerializedProperty sceneNameProp = property.FindPropertyRelative("_sceneName");

        // _sceneAsset 필드 그리기
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, position.height), sceneAssetProp, GUIContent.none);

        // 선택된 SceneAsset을 Scene 이름에 저장
        if (sceneAssetProp.objectReferenceValue != null)
        {
            SceneAsset sceneAsset = sceneAssetProp.objectReferenceValue as SceneAsset;
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameProp.stringValue != sceneName)
            {
                sceneNameProp.stringValue = sceneName; // _sceneName에 씬 이름 저장
                property.serializedObject.ApplyModifiedProperties(); // 변경 사항 저장
            }
        }
        else
        {
            sceneNameProp.stringValue = ""; // SceneAsset이 없으면 빈 문자열로 설정
            property.serializedObject.ApplyModifiedProperties(); // 변경 사항 저장
        }

        EditorGUI.EndProperty();
    }
}