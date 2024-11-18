using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class BuildSpriteUpScaler
{
    public const int SCALE = 5;
    public const string COMPUTE_SHADER_PATH = "Assets/Dev/Script/Editor/SpriteUpScale.compute";
    public int callbackOrder => -10;

    public static readonly string[] Targets = new[]
    {
        "Assets/Dev/Art/Sprite/Building",
        "Assets/Dev/Art/Sprite/Character",
        "Assets/Dev/Art/Sprite/Animal",
        "Assets/Dev/Art/Sprite/Exterior",
        "Assets/Dev/Art/Sprite/Insoo",
        "Assets/Dev/Art/Sprite/Interior",
        "Assets/Dev/Art/Sprite/Object",
        "Assets/Dev/Art/Sprite/Tile",
        "Assets/Dev/Art/Sprite/UGUI",
    };

    [MenuItem("Build/UpscaleTexture")]
    public static void BuildUpscaleTexture()
    {
        string[] allGuid = AssetDatabase.FindAssets("t:texture2D", Targets);
        var computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(COMPUTE_SHADER_PATH);

        Debug.Assert(computeShader);

        foreach (var guid in allGuid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Debug.Assert(texture);

            (int newWidth, int newHeight, int scale) size = GetNewSize(SCALE, texture);
            texture = RescaleTexture(texture, size.newWidth, size.newHeight, computeShader);
            
            SaveTextureAsPNG(texture, path);
        }
        
        var factory = new SpriteDataProviderFactories();
        foreach (var guid in allGuid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Debug.Assert(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            (int newWidth, int newHeight, int scale) size = GetNewSize(SCALE, texture);
            importer.spritePixelsPerUnit *= size.scale;
            importer.maxTextureSize = GetCloserSize(Mathf.Max(size.newHeight, size.newWidth));
            importer.filterMode = FilterMode.Trilinear;
            
            ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();
            ISpriteOutlineDataProvider outlineProvider = dataProvider.GetDataProvider<ISpriteOutlineDataProvider>();
            
            SpriteRect[] spriteRects = dataProvider.GetSpriteRects();

            foreach (SpriteRect spriteRect in spriteRects)
            {
                List<Vector2[]> outlines  = outlineProvider.GetOutlines(spriteRect.spriteID);

                outlines = outlines.Select(arr =>
                {
                    return arr.Select(v => v * (float)size.scale).ToArray();
                }).ToList();
                
                
                outlineProvider.SetOutlines(spriteRect.spriteID, outlines);
            }
            
            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                for (int i = 0; i < spriteRects.Length; i++)
                {
                    Rect rect = spriteRects[i].rect;
                    rect.x *= size.scale;
                    rect.y *= size.scale;
                    rect.width *= size.scale;
                    rect.height *= size.scale;
                    spriteRects[i].rect = rect;
                }
                dataProvider.SetSpriteRects(spriteRects);
            }
            
            dataProvider.Apply();
            importer.SaveAndReimport();
        }
        
        AssetDatabase.Refresh();
    }
    public static int GetCloserSize(int size)
    {
        for (int i = 5; i <= 14; i++)
        {
            int t = Mathf.RoundToInt(Mathf.Pow(2, i));
            if (size < t)
            {
                return t;
            }
        }
        
        Debug.Assert(false);
        return 16384;
    }

    // 텍스처를 PNG 파일로 저장하는 메서드
    public static void SaveTextureAsPNG(Texture2D texture, string path)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }
    public static (int newWidth, int newHeight, int scale) GetNewSize(int scale, Texture2D texture2D)
    {
        while (true)
        {
            if (texture2D.width * scale < 100 || texture2D.height * scale < 100)
            {
                scale *= 2;
            }
            else
            {
                break;
            }
        }
        
        return (texture2D.width * scale, texture2D.height * scale, scale);
        
    }
    
    public static Texture2D RescaleTexture(Texture2D inputTexture, int newWidth, int newHeight, ComputeShader rescaleComputeShader)
    {
        // RenderTexture 설정
        RenderTexture outputTexture = new RenderTexture(newWidth, newHeight, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        // Compute Shader에 변수 설정
        int kernelHandle = rescaleComputeShader.FindKernel("CSMain");
        rescaleComputeShader.SetTexture(kernelHandle, "_InputTexture", inputTexture);
        rescaleComputeShader.SetTexture(kernelHandle, "_OutputTexture", outputTexture);
        rescaleComputeShader.SetInt("_InputWidth", inputTexture.width);
        rescaleComputeShader.SetInt("_InputHeight", inputTexture.height);
        rescaleComputeShader.SetInt("_NewWidth", newWidth);
        rescaleComputeShader.SetInt("_NewHeight", newHeight);
        rescaleComputeShader.SetFloat("_srgb", 1f / 2.2f);

        // Compute Shader 실행
        int threadGroupsX = Mathf.CeilToInt(newWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(newHeight / 8.0f);
        rescaleComputeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

        // RenderTexture 데이터를 Texture2D로 복사
        Texture2D rescaledTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        RenderTexture.active = outputTexture;
        rescaledTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        rescaledTexture.Apply();

        // 리소스 해제
        RenderTexture.active = null;
        outputTexture.Release();

        return rescaledTexture;
    }
}
