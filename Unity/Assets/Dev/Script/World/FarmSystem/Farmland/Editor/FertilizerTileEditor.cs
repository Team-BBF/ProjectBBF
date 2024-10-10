using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

[CustomEditor(typeof(FertilizerTile))]
public class FertilizerTileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var obj = (FertilizerTile)target;


        EditorGUILayout.LabelField("기본 Tile 정보");
        obj.sprite = EditorGUILayout.ObjectField("Sprite", obj.sprite, typeof(Sprite), false) as Sprite;
        obj.color = EditorGUILayout.ColorField("Color", obj.color);
        obj.colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup("ColliderType", obj.colliderType);
        
        
        FarmlandTileEditorUtils.DrawPropertyFarmlandBase(serializedObject);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_buffGrowingSpeed")); 
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    } 
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        FertilizerTile tile = (FertilizerTile)target;
        var tex = FarmlandTileEditorUtils.RenderStaticPreview(tile.sprite, width, height);
        
        return tex ? tex : base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}