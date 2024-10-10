using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class SceneCaptureUtility
{
    public static float CalculateUnit(float resolutionXorY, float ppu)
    {
        /*
         * a = resolutions
         * 1 : ppu = x : a
         * ppu * x = a
         * x = a / ppu
         * a = 1024, ppu = 100, x = 10.24
         * */

        return resolutionXorY / ppu;
    }

    public static void Redraw(float unit, Vector2Int iteration, out Color[,] colors)
    {
        colors = null;

        if (iteration.x < 0) return;
        if (iteration.y < 0) return;

        colors = new Color[iteration.x, iteration.y];


        for (int i = 0; i < iteration.y; i++)
        {
            for (int j = 0; j < iteration.x; j++)
            {
                Color color = UnityEngine.Random.ColorHSV();
                colors[j, i] = color;
            }
        }
    }

    public static void DrawGizmos(Vector3 offset, float unit, Vector2Int iteration, Color[,] colors)
    {
        if (iteration.x < 0) return;
        if (iteration.y < 0) return;

        var color = Color.red;

        Vector3 pos = new Vector3(offset.x + unit * 0.5f, offset.y + -unit * 0.5f, -10f);

        for (int i = 0; i < iteration.y; i++)
        {
            var backupPos = pos;
            for (int j = 0; j < iteration.x; j++)
            {
                color = colors[j, i];
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawCube(pos, new Vector3(unit, unit, 1f));
                pos += new Vector3(unit, 0f, 0f);
            }

            pos = backupPos;
            pos += new Vector3(0f, -unit, 0f);
        }
    }
    
    public static void IteratePosition(Vector3 offset, float unit, Vector2Int iteration, Action<Vector3> callback)
    {
        if (iteration.x < 0) return;
        if (iteration.y < 0) return;
        
        Vector3 pos = new Vector3(offset.x + unit * 0.5f, offset.y + -unit * 0.5f, -10f);

        for (int i = 0; i < iteration.y; i++)
        {
            var backupPos = pos;
            for (int j = 0; j < iteration.x; j++)
            {
                callback?.Invoke(pos);
                pos += new Vector3(unit, 0f, 0f);
            }

            pos = backupPos;
            pos += new Vector3(0f, -unit, 0f);
        }
    }
}