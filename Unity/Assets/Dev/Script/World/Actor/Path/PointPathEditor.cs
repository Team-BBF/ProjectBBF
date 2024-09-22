using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;


#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine.Rendering.VirtualTexturing;

    [CustomEditor(typeof(PatrolPointPath))]
    internal class PointPathEditor : Editor
    {
        private Rect _sceneSize;

        private Vector2 _bttonSize => new Vector2(150f, 100f);
        private Vector2 _bttonOffset => new Vector2(15f, 15f);
        public Vector2 PointBoxSize = new Vector2(50f, 50f);

        private bool _isAdding;

        private PatrolPointPath _pointPath;

        private void OnEnable()
        {
            _pointPath = serializedObject.targetObject as PatrolPointPath;
            _pointPath.SelectedGuid = "";
        }

        private void OnDisable()
        {
            _isAdding = false;
        }

        private void GeneratePath()
        {
            var startPoint = _pointPath.Points.Find(x => x.IsStartPoint);

            if (startPoint == null)
            {
                Debug.LogError("시작 Patroll point가 없습니다!");
                return;
            }

            List<PatrolPoint> patrollPoints = new List<PatrolPoint>(_pointPath.Points.Count);
            HashSet<string> stacks = new HashSet<string>();

            PointInfo currentPoint = startPoint;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < 1000 && patrollPoints.Count <= 1000)
            {
                stacks.Add(currentPoint.Guid);
                string guid = null;
                foreach (var nextGuid in currentPoint.Neighbors)
                {
                    if (nextGuid != currentPoint.Guid && stacks.Contains(nextGuid) == false)
                    {
                        guid = nextGuid;
                        break;
                    }
                }

                if (guid == null)
                {
                    patrollPoints.Add(new PatrolPoint()
                    {
                        _interactiveDecoratedPoint = currentPoint._interactiveDecoratedPoint,
                        _position = currentPoint.Position
                    });
                    break;
                }

                var nextPoint = _pointPath.Points.Find(x => x.Guid == guid);

                patrollPoints.Add(new PatrolPoint()
                {
                    _interactiveDecoratedPoint = currentPoint._interactiveDecoratedPoint,
                    _position = currentPoint.Position,
                    _nextPosition = nextPoint.Position
                });

                currentPoint = nextPoint;
            }

            watch.Stop();

            _pointPath._patrollPoints = patrollPoints;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save Path"))
            {
                Save();
            }
            
        }
        
        private void OnSceneGUI()
        {
            if (((PatrolPointPath)target).Edit is false) return;
            
            Undo.RecordObject(target, "Path set");
            
            _sceneSize = SceneView.lastActiveSceneView.position;
            
            var pointPath = _pointPath;
            
            if (pointPath.Points.Count == 0)
            {
                pointPath.Points.Add(new PointInfo()
                {
                    Guid = GUID.Generate().ToString(),
                    Position = (Vector2)pointPath.transform.position,
                    Neighbors = new List<string>(),
                    IsStartPoint = true
                });
            }

            foreach (var point in pointPath.Points)
            {
                ValidatePoint(point);
                DrawPoint(point);
            }

            if (OnPointClickAdd() == false)
            {
                OnClickBoundBox();
            }

            var selected = pointPath.Selected;

            if (selected != null)
            {
                DrawButton(selected);
            }

            SceneView.RepaintAll();
        }

        private bool OnPointClickAdd()
        {
            var pointPath = _pointPath;
            var selected = pointPath.Selected;
            var mousePos = UnityEngine.Event.current.mousePosition;

            if (_isAdding && selected != null)
            {
                var e = UnityEngine.Event.current;
                Color c = Color.green;
                var pos = HandleUtility.GUIPointToWorldRay(mousePos).origin;
                Handles.DrawDottedLine(selected.Position, pos, 4.0f);

                InteractiveDecoratedPoint interactiveDecoratedPoint = GetInteractiveObject(mousePos);
                if (interactiveDecoratedPoint != null)
                {
                    c = Color.yellow;
                }

                c.a = 0.5f;

                Handles.color = c;

                Handles.DotHandleCap(
                    1,
                    pos,
                    Quaternion.identity,
                    0.2f,
                    EventType.Repaint
                );

                if (e.type == EventType.KeyDown && e.control)
                {
                    if (interactiveDecoratedPoint)
                    {
                        pos = interactiveDecoratedPoint.InteractingPosition;
                    }

                    var newInfo = OnAddPoint(pos);
                    newInfo._interactiveDecoratedPoint = interactiveDecoratedPoint;

                    selected.Neighbors.Add(newInfo.Guid);
                    newInfo.Neighbors.Add(selected.Guid);

                    pointPath.Selected = newInfo;
                    _isAdding = false;

                    return true;
                }
            }

            return false;
        }

        private void OnClickBoundBox()
        {
            var pointPath = _pointPath;

            var e = UnityEngine.Event.current;
            if (e.type == EventType.KeyDown && e.control)
            {
                float minDis = Mathf.Infinity;
                PointInfo minPointInfo = null;

                foreach (var point in pointPath.Points)
                {
                    var newPos = HandleUtility.WorldToGUIPoint(point.Position);
                    Bounds a = new Bounds(UnityEngine.Event.current.mousePosition, PointBoxSize);

                    if (a.Contains(newPos))
                    {
                        var dis = ((Vector2)point.Position - UnityEngine.Event.current.mousePosition).sqrMagnitude;

                        if (dis < minDis)
                        {
                            minPointInfo = point;
                            minDis = dis;
                        }
                    }
                }

                if (minPointInfo != null)
                {
                    pointPath.Selected = minPointInfo;
                }
            }
        }

        private void Save()
        {
            GeneratePath();
            EditorUtility.SetDirty(serializedObject.targetObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(serializedObject.targetObject);
        }

        [CanBeNull]
        private InteractiveDecoratedPoint GetInteractiveObject(Vector2 sceneViewPos)
        {
            var ray = HandleUtility.GUIPointToWorldRay(sceneViewPos);
            var resutls = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);


            foreach (var result in resutls)
            {
                if (result.transform.TryGetComponent<InteractiveDecoratedPoint>(out var com))
                {
                    return com;
                }
            }

            return null;
        }

        private void ValidatePoint(PointInfo point)
        {
            var pointPath = _pointPath;
            List<string> guids = new List<string>(2);
            foreach (var guid in point.Neighbors)
            {
                if (pointPath.Points.Any(x => x.Guid == guid))
                {
                    guids.Add(guid);
                }
            }

            if (guids.Count != point.Neighbors.Count)
            {
                point.Neighbors = guids;
            }
        }

        private void DrawPoint(PointInfo point)
        {
            var size = 0.2f;

            if (point._interactiveDecoratedPoint)
            {
                Handles.color = Color.yellow;
            }
            else if (point.IsStartPoint)
            {
                Handles.color = Color.red;
            }
            else
            {
                Handles.color = Color.green;
            }

            Handles.DotHandleCap(
                1,
                point.Position,
                Quaternion.identity,
                size,
                EventType.Repaint
            );

            var pos = Handles.FreeMoveHandle(
                point.Position,
                size,
                Vector3.zero,
                Handles.RectangleHandleCap
            );

            var pointPath = _pointPath;
            foreach (string guid in point.Neighbors)
            {
                var foundPoint = pointPath.Points.Find(x => x.Guid == guid);

                if (foundPoint == null)
                {
                    continue;
                }

                Handles.color = Color.green;
                Handles.DrawLine(foundPoint.Position, point.Position, 2f);
            }

            if (Mathf.Approximately((point.Position - pos).sqrMagnitude, 0f) == false)
            {
                point.Position = pos;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawButton(PointInfo point)
        {
            var pos = WorldPointToViewportSpace(point.Position);
            var width = _sceneSize.width;
            var height = _sceneSize.height;

            var rect = new Rect(
                pos.x * width + _bttonOffset.x,
                height - (pos.y * height + _bttonOffset.y),
                _bttonSize.x,
                _bttonSize.y
            );

            GUILayout.BeginArea(rect);

            if ((point.IsStartPoint is false && point.Neighbors.Count <= 1 || point.IsStartPoint && point.Neighbors.Count == 0) && GUILayout.Button("Add"))
            {
                _isAdding = true;
            }

            if (point.IsStartPoint == false && point.Neighbors.Count <= 1 && GUILayout.Button("Remove"))
            {
                OnRemovePoint();
            }

            if (point.IsStartPoint == false && point.Neighbors.Count <= 1)
            {
                if (GUILayout.Button("Set Start"))
                {
                    OnStartPoint();
                }
            }

            if (point._interactiveDecoratedPoint)
            {
                if (GUILayout.Button("Move to object"))
                {
                    point.Position = point._interactiveDecoratedPoint.InteractingPosition;
                }
            }

            if (point._interactiveDecoratedPoint)
            {
                if (GUILayout.Button("clear object"))
                {
                    point._interactiveDecoratedPoint = null;
                }
            }

            var arr = Physics2D.OverlapCircleAll(point.Position, 1f)
                .Select(x => x.GetComponent<InteractiveDecoratedPoint>())
                .Where(x => x != null)
                .ToList();
            if (point._interactiveDecoratedPoint == false && arr.Any() && GUILayout.Button("Set object"))
            {
                point._interactiveDecoratedPoint = arr[0];
                point.Position = point._interactiveDecoratedPoint.InteractingPosition;
            }

            GUILayout.EndArea();
        }

        private Vector2 WorldPointToScreenSpace(Vector3 pos)
        {
            Debug.Assert(SceneView.lastActiveSceneView != null);

            var camera = SceneView.lastActiveSceneView.camera;

            pos = camera.WorldToViewportPoint(pos);

            pos = new Vector2(
                pos.x * _sceneSize.width,
                _sceneSize.height - pos.y * _sceneSize.height
            );


            return pos * camera.orthographicSize;
        }

        private Vector2 WorldPointToViewportSpace(Vector3 pos)
        {
            Debug.Assert(SceneView.lastActiveSceneView != null);

            var camera = SceneView.lastActiveSceneView.camera;
            return camera.WorldToViewportPoint(pos);
        }

        private Vector2 ScreenPointToWorldSpace(Vector2 pos)
        {
            Debug.Assert(SceneView.lastActiveSceneView != null);

            var camera = SceneView.lastActiveSceneView.camera;

            pos = new Vector2(
                pos.x / _sceneSize.width,
                1f - pos.y / _sceneSize.height
            );

            return camera.ViewportToWorldPoint(pos);
        }

        private Ray GetScreenRay(Vector2 sceneViewPos)
        {
            var camera = SceneView.lastActiveSceneView.camera;

            sceneViewPos = new Vector2(
                sceneViewPos.x / _sceneSize.width,
                1f - sceneViewPos.y / _sceneSize.height
            );

            return camera.ViewportPointToRay(sceneViewPos);
        }

        private bool OverlapPoint(Vector2 aPos, Vector2 aSize, Vector2 bPos, Vector2 bSize)
        {
            Bounds a = new Bounds(aPos, aSize);
            Bounds b = new Bounds(bPos, bSize);

            return a.Intersects(b);
        }

        private PointInfo OnAddPoint(Vector2 worldPos)
        {
            var pointPath = _pointPath;


            PointInfo info = new PointInfo()
            {
                Guid = GUID.Generate().ToString(),
                Position = worldPos,
                Neighbors = new List<string>()
            };

            pointPath.Points.Add(info);

            return info;
        }

        private void OnRemovePoint()
        {
            var pointPath = _pointPath;
            var index = pointPath.Points.FindIndex(x => x.Guid == pointPath.SelectedGuid);
            pointPath.Points.RemoveAt(index);
        }

        private void OnStartPoint()
        {
            var pointPath = _pointPath;

            pointPath.Points.ForEach(x => x.IsStartPoint = false);

            var selected = pointPath.Selected;
            if (selected != null)
            {
                selected.IsStartPoint = true;
            }
        }
    }
#endif