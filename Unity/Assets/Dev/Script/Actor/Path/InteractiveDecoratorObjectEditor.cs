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

            Handles.DotHandleCap(
                1,
                obj.InteractingPositionWorld,
                Quaternion.identity,
                0.1f,
                EventType.Repaint
            );
            
            var pos = Handles.FreeMoveHandle(
                obj.InteractingPositionWorld,
                0.1f,
                Vector3.zero,
                Handles.RectangleHandleCap
            );

            obj.InteractingPositionWorld = pos;
        }
    }
#endif