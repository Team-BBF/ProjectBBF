using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

public static class FarmlandTileEditorUtils
{
    private static Type GetType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null)
            return type;

        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
        }

        return null;
    }

    public static void DrawPropertyFarmlandBase(SerializedObject serializedObject)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Farmland Tile 정보");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dropItem")); 
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dropItemCount")); 
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_requireTools")); 
    }

    public static Texture2D RenderStaticPreview(Sprite sprite, int width, int height)
    {
        if (sprite is null) return null;
        Type t = GetType("UnityEditor.SpriteUtility");

        if (t is null) return null;
        
        MethodInfo method = t.GetMethod(
            "RenderStaticPreview", 
            new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) }
            );

        if (method is null) return null;
        
        object ret = method.Invoke(
            "RenderStaticPreview",
            new object[] { sprite, Color.white, width, height }
            );

        if (ret is not Texture2D tex) return null;
        
        return tex;
    }
}