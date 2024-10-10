using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

[CustomEditor(typeof(CultivationTile))]
public class CultivationTileEditor : RuleTileEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    } 
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        CultivationTile tile = (CultivationTile)target;
        var tex = FarmlandTileEditorUtils.RenderStaticPreview(tile.m_DefaultSprite, width, height);
        
        return tex ? tex : base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}
