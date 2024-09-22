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
    public static readonly int Up_Idle          = Animator.StringToHash("Up_Idle");
    public static readonly int Down_Idle        = Animator.StringToHash("Down_Idle");
    public static readonly int Left_Idle        = Animator.StringToHash("Left_Idle");
    public static readonly int Right_Idle       = Animator.StringToHash("Right_Idle");
    public static readonly int LeftUp_Idle      = Animator.StringToHash("LeftUp_Idle");
    public static readonly int RightUp_Idle     = Animator.StringToHash("RightUp_Idle");

    public static readonly int Up_Move          = Animator.StringToHash("Up_Move");
    public static readonly int Down_Move        = Animator.StringToHash("Down_Move");
    public static readonly int Left_Move        = Animator.StringToHash("Left_Move");
    public static readonly int Right_Move       = Animator.StringToHash("Right_Move");
    public static readonly int LeftUp_Move      = Animator.StringToHash("LeftUp_Move");
    public static readonly int RightUp_Move     = Animator.StringToHash("RightUp_Move");

    public static readonly int Up_Sprint        = Animator.StringToHash("Up_Sprint");
    public static readonly int Down_Sprint      = Animator.StringToHash("Down_Sprint");
    public static readonly int Left_Sprint      = Animator.StringToHash("Left_Sprint");
    public static readonly int Right_Sprint     = Animator.StringToHash("Right_Sprint");
    public static readonly int LeftUp_Sprint    = Animator.StringToHash("LeftUp_Sprint");
    public static readonly int RightUp_Sprint   = Animator.StringToHash("RightUp_Sprint");

    /*
     * Action
     */
    public static readonly int Hoe              = Animator.StringToHash("Hoe");
    public static readonly int Pickaxe          = Animator.StringToHash("Pickaxe");
    public static readonly int Collect          = Animator.StringToHash("Collect");


    [Serializable]
    public enum Movement
    {
        Idle,
        Walk,
        Sprint
    }

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
        Hoe,
        Pickaxe,
        Collect,
    }

    public static int GetAniHash(Action action)
    {
        switch (action)
        {
            case Action.Hoe: return Hoe;
            case Action.Pickaxe: return Pickaxe;
            case Action.Collect: return Collect;
        }

        throw new ArgumentException($"Invalid combination of Action: {action}");
    }

    public static int GetAniHash(Movement movement, Direction direction)
    {
        switch (movement)
        {
            case Movement.Idle:
                switch (direction)
                {
                    case Direction.Up: return Up_Idle;
                    case Direction.Down: return Down_Idle;
                    case Direction.Left: return Left_Idle;
                    case Direction.Right: return Right_Idle;
                    case Direction.LeftUp: return LeftUp_Idle;
                    case Direction.RightUp: return RightUp_Idle;
                }
                break;

            case Movement.Walk:
                switch (direction)
                {
                    case Direction.Up: return Up_Move;
                    case Direction.Down: return Down_Move;
                    case Direction.Left: return Left_Move;
                    case Direction.Right: return Right_Move;
                    case Direction.LeftUp: return LeftUp_Move;
                    case Direction.RightUp: return RightUp_Move;
                }
                break;

            case Movement.Sprint:
                switch (direction)
                {
                    case Direction.Up: return Up_Sprint;
                    case Direction.Down: return Down_Sprint;
                    case Direction.Left: return Left_Sprint;
                    case Direction.Right: return Right_Sprint;
                    case Direction.LeftUp: return LeftUp_Sprint;
                    case Direction.RightUp: return RightUp_Sprint;
                }
                break;
        }

        throw new ArgumentException($"Invalid combination of movement: {movement} and direction: {direction}");
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
        if (TryGetMovement(myClip.name, out Movement myMovement) is false) return;


        for (int i = 0; i < list.Count; i++)
        {
            var pair = list[i];
            var clip = list[i].Key;

            if (TryGetDirection(clip.name, out Direction direction) is false) continue;
            if (TryGetMovement(clip.name, out Movement movement) is false) continue;

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
        // 예: @Clip_Up_Idle.anim
        // 파일 이름이 특정 패턴을 따르는지 검사
        return fileName.StartsWith("@Clip_") &&
               (fileName.Contains("_Up_") || fileName.Contains("_Down_") ||
                fileName.Contains("_Right_") || fileName.Contains("_Left_") ||
                fileName.Contains("_RightUp_") || fileName.Contains("_LeftUp_")) &&
               (fileName.Contains("_Idle") || fileName.Contains("_Move") || fileName.Contains("_Sprint"));
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

    private static bool TryGetMovement(string fileName, out Movement movement)
    {
        movement = default;

        if (fileName.Contains("_Idle"))
        {
            movement = Movement.Idle;
        }
        else if (fileName.Contains("_Move"))
        {
            movement = Movement.Walk;
        }
        else if (fileName.Contains("_Sprint"))
        {
            movement = Movement.Sprint;
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