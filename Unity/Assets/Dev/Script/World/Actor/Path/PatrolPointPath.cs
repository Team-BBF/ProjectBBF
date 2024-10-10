using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

#region Editor

#if UNITY_EDITOR
using System.IO;
using UnityEditor;

[System.Serializable]
internal class PointInfo
{
    [SerializeField] internal List<string> Neighbors = new();
    [SerializeField] internal string Guid;
    [SerializeField] internal bool IsStartPoint;

    [SerializeField] internal Vector3 Position;
    [SerializeField] internal InteractiveDecoratedPoint _interactiveDecoratedPoint;
}
#endif

#endregion

[System.Serializable]
public class PatrolPoint
{
    [SerializeField] internal InteractiveDecoratedPoint _interactiveDecoratedPoint;
    [SerializeField] internal Vector3 _position;
    [SerializeField] internal Vector3 _nextPosition;

    public InteractiveDecoratedPoint InteractiveDecoratedPoint => _interactiveDecoratedPoint;
    public Vector3 Position => _position;
    public Vector3 NextPosition => _nextPosition;
}

[ExecuteInEditMode]
public class PatrolPointPath : MonoBehaviour
{
    #region Editor

#if UNITY_EDITOR
    [MenuItem("GameObject/ProjectBBF/동선 프리팹", false)]
    private static void CreateMenuItem()
    {
        string filePath = EditorUtility.SaveFilePanelInProject
        (
            "프리팹 폴더",
            "New Path",
            "prefab",
            "저장할 동선 프리팹 이름 입력",
            "Assets/Dev/Prefab/Path"
        );

        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }
        
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        GameObject go = new GameObject(fileName);
        go.AddComponent<PatrolPointPath>();

        GameObjectUtility.SetParentAndAlign(go, Selection.activeGameObject);
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, filePath, InteractionMode.UserAction);

        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

        Selection.activeObject = go;
    }

    [SerializeField] [HideInInspector] internal List<PointInfo> Points = new();
    [SerializeField] [HideInInspector] internal string SelectedGuid;

    [SerializeField] internal bool Edit;

    [CanBeNull]
    internal PointInfo Selected
    {
        get
        {
            var info = Points.Find(x => x.Guid == SelectedGuid);

            return info;
        }
        set => SelectedGuid = value.Guid;
    }
#endif

    #endregion

    [SerializeField] internal List<PatrolPoint> _patrollPoints = new();

    public IReadOnlyList<PatrolPoint> PatrollPoints => _patrollPoints;

    public void SetPatrollPoints(IReadOnlyList<PatrolPoint> patrollPoints)
    {
        _patrollPoints = new List<PatrolPoint>(patrollPoints);
    }
}