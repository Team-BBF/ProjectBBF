using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
    using UnityEditor;
    
    [CustomEditor(typeof(InteractiveDecoratedPoint))]
    [CanEditMultipleObjects]
    internal class InteractiveDecoratorObjectEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var obj = target as InteractiveDecoratedPoint;

            if (obj.InteractingPosition == Vector2.zero)
            {
                obj.InteractingPosition = obj.transform.position;
            }

            Handles.DotHandleCap(
                1,
                obj.InteractingPosition,
                Quaternion.identity,
                0.1f,
                EventType.Repaint
            );
            
            var pos = Handles.FreeMoveHandle(
                obj.InteractingPosition,
                0.1f,
                Vector3.zero,
                Handles.RectangleHandleCap
            );

            obj.InteractingPosition = pos;
        }
    }
#endif