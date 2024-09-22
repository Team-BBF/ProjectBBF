using System;
using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class SceneCapture : MonoBehaviour
{
    private Vector2 _offset;
    private Camera _camera;
    private string _filePath;
    private string _uniqueFileName;
    private Vector2Int _resoultion;
    private int _ppu;

    private Vector2Int _iteration;

    private int _captureDelay = 1;

    [field: SerializeField, HideInInspector] private bool _onPlay;

    public bool OnPlay
    {
        get => _onPlay;
        set => _onPlay = value;
    }

    public Vector2 Offset
    {
        get => _offset;
        set => _offset = value;
    }

    public Camera Camera
    {
        get => _camera;
        set => _camera = value;
    }

    public string FilePath
    {
        get => _filePath;
        set => _filePath = value;
    }

    public string UniqueFileName
    {
        get => _uniqueFileName;
        set => _uniqueFileName = value;
    }

    public Vector2Int Resoultion
    {
        get => _resoultion;
        set => _resoultion = value;
    }

    public Vector2Int Iteration
    {
        get => _iteration;
        set => _iteration = value;
    }

    public int PPU
    {
        get => _ppu;
        set => _ppu = value;
    }

    public int CaptureDelay
    {
        get => _captureDelay;
        set => _captureDelay = value;
    }

    private float Unit => SceneCaptureUtility.CalculateUnit(_resoultion.x, _ppu);
    private float HalfUnit => Unit * 0.5f;

    public void Capture()
    {
        _camera.farClipPlane = 999999f;
        _camera.nearClipPlane = -999999f;
        _camera.orthographicSize = Unit * 0.5f; // orthographicSize 화면 세로 높이의 절반이기 때문에 2로 나눔
        _camera.aspect = 1f;
        _camera.transform.position = new Vector3(_offset.x + HalfUnit,
            _offset.y + -HalfUnit, -10f);

        RenderTexture renderTexture = _camera.targetTexture;
        _camera.targetTexture = null;
        renderTexture.Release();
        renderTexture.width = _resoultion.x;
        renderTexture.height = _resoultion.y;
        if (renderTexture.Create() == false) Debug.LogError("SceneCapture에서 RenderTexture의 resizing을 실패함.");
        _camera.targetTexture = renderTexture;
        
        
        // 에디터가 이미 Play 모드에 있는지 확인합니다
        if (!EditorApplication.isPlaying)
        {
            // Play 모드로 전환합니다
            EditorApplication.isPlaying = true;
        }
    }

    private void Awake()
    {
        if (OnPlay == false) return;
        OnPlay = false;

        float backupScale = 0f;
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset asset)
        {
            backupScale = asset.renderScale;
asset.renderScale= 1;
        }
        else
        {
            Debug.LogError("UniversalRenderPipelineAsset을 불러오지 못했음");
        }
        
        UniTask.Create(async () =>
        {
            await UniTask.Delay(300);

            StringBuilder errorBuilder = new StringBuilder();
            StringBuilder infoBuilder = new StringBuilder();
            bool onError = false;

            for (int i = 0; i < _iteration.y; i++)
            {
                var backupPos = _camera.transform.position;
                for (int j = 0; j < _iteration.x; j++)
                {
                    await UniTask.Delay(_captureDelay);
                    _camera.transform.position += new Vector3(Unit, 0f, 0f);

                    var texture2d = CopyRenderToTexture2D();
                    Gamma2Linear(ref texture2d);
                    //texture2d = ScaleTexture(texture2d, _resoultion.x, _resoultion.y);
                    (bool, string) result = SaveCapturedTexture(texture2d, $"Map_{_uniqueFileName}_{j}_{i}");

                    if (result.Item1)
                    {
                        infoBuilder.AppendLine(result.Item2);
                    }
                    else
                    {
                        onError = true;
                        errorBuilder.AppendLine(result.Item2);
                    }
                }

                _camera.transform.position = backupPos;
                _camera.transform.position += new Vector3(0f, -Unit, 0f);
            }

            if (onError)
            {
                Debug.LogError(errorBuilder.ToString());
            }
            
            Debug.Log(infoBuilder.ToString());

            EditorApplication.isPlaying = false;

        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset asset1)
        {
            asset1.renderScale = backupScale;
        }
        else
        {
            Debug.LogError("UniversalRenderPipelineAsset을 불러오지 못했음");
        }
        });
        
        
    }

    private Texture2D CopyRenderToTexture2D()
    {
        Texture2D texture2D = null;


        RenderTexture renderTexture = _camera.targetTexture;
        RenderTexture originalRenderTexture = _camera.targetTexture;
        _camera.targetTexture = renderTexture;

        RenderTexture.active = renderTexture;
        if (texture2D == null || texture2D.width != renderTexture.width || texture2D.height != renderTexture.height)
        {
            texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        }

        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = null;
        _camera.targetTexture = originalRenderTexture;

        return texture2D;
    }

    private void Gamma2Linear(ref Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture2D is null!");
            return;
        }

        Color[] pixels = texture.GetPixels();

        Color[] linearPixels = new Color[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            linearPixels[i] = sRGBToLinear(pixels[i]);
        }

        texture.SetPixels(linearPixels);
        texture.Apply();
    }

    private Color sRGBToLinear(Color color)
    {
        return new Color(
            Mathf.Pow(color.r, 1f / 2.2f),
            Mathf.Pow(color.g, 1f / 2.2f),
            Mathf.Pow(color.b, 1f / 2.2f),
            color.a
        );
    }

    private Texture2D CropTexture(Texture2D original, int x, int y, int width, int height)
    {
        // 원본 텍스처의 지정된 영역에서 픽셀을 가져옴
        Color[] pixels = original.GetPixels(x, y, width, height);

        // 새로운 텍스처를 생성하고, 해당 픽셀을 설정
        Texture2D croppedTexture = new Texture2D(width, height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    public Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        // 새로운 크기의 Texture2D 생성
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);

        // 스케일 비율 계산
        float scaleX = (float)source.width / targetWidth;
        float scaleY = (float)source.height / targetHeight;

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                // 원본 텍스처에서의 좌표 계산 (가장 가까운 픽셀을 선택)
                int pixelX = Mathf.FloorToInt(x * scaleX);
                int pixelY = Mathf.FloorToInt(y * scaleY);

                // 원본 텍스처의 색상 가져오기
                Color color = source.GetPixel(pixelX, pixelY);

                // 결과 텍스처에 색상 설정
                result.SetPixel(x, y, color);
            }
        }

        // 텍스처 적용
        result.Apply();
        return result;
    }
    private (bool, string) SaveCapturedTexture(Texture2D texture2D, string fileName)
    {
        byte[] pngData = texture2D.EncodeToPNG();

        string target = "Assets";

        var newFilePath = _filePath;
        if (_filePath.StartsWith(target))
        {
            newFilePath = newFilePath.Substring(target.Length);
        }

        string path = Application.dataPath + newFilePath + "\\" + fileName + ".png";
        try
        {
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
                return (true, "Texture2D saved as PNG at: " + path);
            }
            else
            {
                return (false, "Failed to encode Texture2D to PNG.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        return (false, "");
    }
}
#endif