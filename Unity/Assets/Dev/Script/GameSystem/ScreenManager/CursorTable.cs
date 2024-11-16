


using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ProjectBBF/Data/Screen/CursorTable", fileName = "Dat_CursorTable")]
public class CursorTable : ScriptableObject
{
    [Serializable]
    public struct CursorSet
    {
        public CursorType Type;
        public Texture2D Texture2D;
    }

    [SerializeField] private List<CursorSet> _list;

    public IReadOnlyList<CursorSet> List => _list;
}