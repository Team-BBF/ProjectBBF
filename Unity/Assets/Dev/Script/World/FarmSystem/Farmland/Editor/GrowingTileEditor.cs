
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.Tilemaps;

[CustomEditor(typeof(PlantTile))]
public class GrowingTileEditor : AnimatedTileEditor
{
    public override void OnInspectorGUI()
    {
        var obj = (PlantTile)target;
        obj.DefaultEditorSprite = EditorGUILayout.ObjectField("Preview", obj.DefaultEditorSprite, typeof(Sprite), false) as Sprite;
        base.OnInspectorGUI();

        FarmlandTileEditorUtils.DrawPropertyFarmlandBase(serializedObject);
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        var obj = (PlantTile)target;

        Sprite spr = obj.DefaultEditorSprite;
        
        if (spr == false && obj.m_AnimatedSprites.Length > 0)
        {
            spr = obj.m_AnimatedSprites[0];
        }

        var tex = FarmlandTileEditorUtils.RenderStaticPreview(spr, width, height);
        
        return tex ? tex : base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}
