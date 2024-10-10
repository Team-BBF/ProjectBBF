using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(PlatformTile))]
public class PlatformTileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlatformTile obj = (PlatformTile)target;

        EditorGUILayout.LabelField("기본 Tile 정보");
        
        obj.sprite = EditorGUILayout.ObjectField("Sprite", obj.sprite, typeof(Sprite), false) as Sprite;
        obj.color = EditorGUILayout.ColorField("Color", obj.color);
        obj.colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("ColliderType", obj.colliderType);



        //EditorGUILayout.Space();
        //EditorGUILayout.LabelField("Farmland Tile 정보");
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("_requireTools")); 
        
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(obj);
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        PlatformTile tile = (PlatformTile)target;
        var tex = FarmlandTileEditorUtils.RenderStaticPreview(tile.sprite, width, height);
        
        return tex ? tex : base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}