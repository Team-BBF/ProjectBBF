using System;
using MyBox;
using UnityEngine;
using System.Collections.Generic;
using static AnimationActorKey;
using Unity.VisualScripting;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public static class AnimationActorKey
{
    /*
     * Movement
     */
    public static readonly int Up          = Animator.StringToHash("Up");
    public static readonly int Down        = Animator.StringToHash("Down");
    public static readonly int Left        = Animator.StringToHash("Left");
    public static readonly int Right       = Animator.StringToHash("Right");
    public static readonly int LeftUp      = Animator.StringToHash("LeftUp");
    public static readonly int RightUp     = Animator.StringToHash("RightUp");

    /*
     * Action
     */
    public static readonly int Hoe              = Animator.StringToHash("Hoe");
    public static readonly int Pickaxe          = Animator.StringToHash("Pickaxe");
    public static readonly int Collect          = Animator.StringToHash("Collect");
    public static readonly int WaterSpray       = Animator.StringToHash("WaterSpray");
    public static readonly int Bakery_Knead     = Animator.StringToHash("Bakery_Knead");
    public static readonly int Bakery_Oven      = Animator.StringToHash("Bakery_Oven");
    public static readonly int Bakery_Additive  = Animator.StringToHash("Bakery_Additive");
    public static readonly int Bakery_Additive_Complete  = Animator.StringToHash("Bakery_Additive_Complete");
    public static readonly int Idle             = Animator.StringToHash("Idle");
    public static readonly int Move             = Animator.StringToHash("Move");
    public static readonly int Plant            = Animator.StringToHash("Plant");

    [Serializable]
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        LeftUp,
        RightUp
    }

    
    [Serializable]
    public enum Action
    {
        None,
        Hoe,
        Pickaxe,
        Collect,
        WaterSpray,
        Bakery_Knead,
        Bakery_Oven,
        Bakery_Additive,
        Bakery_Additive_Complete,
        Idle,
        Move,
        Plant,
    }

    public static int GetAniHash(Action action)
    {
        switch (action)
        {
            case Action.None: return -1;
            case Action.Hoe: return Hoe;
            case Action.Pickaxe: return Pickaxe;
            case Action.Collect: return Collect;
            case Action.WaterSpray: return WaterSpray;
            case Action.Bakery_Knead: return Bakery_Knead;
            case Action.Bakery_Oven: return Bakery_Oven;
            case Action.Bakery_Additive: return Bakery_Additive;
            case Action.Bakery_Additive_Complete: return Bakery_Additive_Complete;
            case Action.Idle: return Idle; 
            case Action.Move: return Move;
            case Action.Plant: return Plant;
            default:
                Debug.Assert(false, $"정의되지 않은 Action({action})");
                return -1;
        }
    }
    
    public static int GetAniHash(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Up;
            case Direction.Down: return Down;
            case Direction.Left: return Left;
            case Direction.Right: return Right;
            case Direction.LeftUp: return LeftUp;
            case Direction.RightUp: return RightUp;
        }

        throw new ArgumentException($"Invalid combination of direction: {direction}");
    }

    public static (int action, int dir) GetAniHash(Action action, Direction direction)
    {
        return (GetAniHash(action), GetAniHash(direction));
    }
}

#if UNITY_EDITOR
public static class ActorAnimationContextMenu
{
    [MenuItem("Assets/Set ActorAnimation")]
    private static void Action()
    {
        string folderPath =
            EditorUtility.OpenFolderPanel("Select a file", "Assets/Dev/Art/UAnimation", "Assets/Dev/Art/UAnimation");
        var clips = GetClips(folderPath);

        AnimatorOverrideController animator = Selection.activeObject as AnimatorOverrideController;

        if (animator is null) return;

        List<KeyValuePair<AnimationClip, AnimationClip>> list = animator.animationClips.Select(x =>
        {
            return new KeyValuePair<AnimationClip, AnimationClip>(x, null);
        }).ToList();

        foreach (var clip in clips)
        {
            SetClip(clip, list);
        }

        animator.ApplyOverrides(list);
        EditorUtility.SetDirty(animator);
    }

    private static void SetClip(AnimationClip myClip, List<KeyValuePair<AnimationClip, AnimationClip>> list)
    {

        if (TryGetDirection(myClip.name, out Direction myDir) is false) return;
        if (TryGetMovement(myClip.name, out AnimationActorKey.Action myMovement) is false) return;


        for (int i = 0; i < list.Count; i++)
        {
            var pair = list[i];
            var clip = list[i].Key;

            if (TryGetDirection(clip.name, out Direction direction) is false) continue;
            if (TryGetMovement(clip.name, out AnimationActorKey.Action movement) is false) continue;

            if (myDir == direction && myMovement == movement)
            {
                list[i] = new KeyValuePair<AnimationClip, AnimationClip>(clip, myClip);
            }
        }
    }

    private static List<AnimationClip> GetClips(string folderPath)
    {
        List<AnimationClip> clips = new();

        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("유효하지 않은 경로");
            return clips;
        }

        string[] animationPaths = Directory.GetFiles(folderPath, "*.anim", SearchOption.AllDirectories);

        // 애니메이션 파일을 필터링
        foreach (string path in animationPaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (IsValidAnimationFile(fileName))
            {
                string assetPath = GetAssetPathFromFile(path);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

                if (clip != null)
                {
                    clips.Add(clip);
                }
                else
                {
                    Debug.LogError("Failed to load Animation Clip: " + assetPath);
                }
            }
        }

        return clips;
    }

    private static bool IsValidAnimationFile(string fileName)
    {
        // 예: @Clip_Up.anim
        // 파일 이름이 특정 패턴을 따르는지 검사
        return fileName.StartsWith("@Clip_") &&
               (fileName.Contains("_Up_") || fileName.Contains("_Down_") ||
                fileName.Contains("_Right_") || fileName.Contains("_Left_") ||
                fileName.Contains("_RightUp_") || fileName.Contains("_LeftUp_")) &&
               (fileName.Contains("Idle") || fileName.Contains("Move"));
    }
    private static bool TryGetDirection(string fileName, out Direction direction)
    {
        direction = default;

        if (fileName.Contains("_Up_"))
        {
            direction = Direction.Up;
        }
        else if (fileName.Contains("_Down_"))
        {
            direction = Direction.Down;
        }
        else if (fileName.Contains("_Left_"))
        {
            direction = Direction.Left;
        }
        else if (fileName.Contains("_Right_"))
        {
            direction = Direction.Right;
        }
        else if (fileName.Contains("_LeftUp_"))
        {
            direction = Direction.LeftUp;
        }
        else if (fileName.Contains("_RightUp_"))
        {
            direction = Direction.RightUp;
        }
        else
        {
            return false; // 방향을 찾지 못했음
        }

        return true;
    }

    private static bool TryGetMovement(string fileName, out AnimationActorKey.Action movement)
    {
        movement = default;

        if (fileName.Contains("_Idle"))
        {
            movement = AnimationActorKey.Action.Idle;
        }
        else if (fileName.Contains("_Move"))
        {
            movement = AnimationActorKey.Action.Move;
        }
        else
        {
            return false; // 동작 상태를 찾지 못했음
        }

        return true;
    }

    private static string GetAssetPathFromFile(string filePath)
    {
        // 파일 경로를 AssetDatabase의 경로로 변환
        string relativePath = "Assets" + filePath.Substring(Application.dataPath.Length);
        return relativePath;
    }
}
#endif