using System;
using System.Collections;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

public class MapEd : MonoBehaviour
{
    [SerializeField] private bool _applySilhouette;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Sprite _silhouette;

    public const string PERSISTENCE_KEY = "MapEd";

    private void Start()
    {
        if (_applySilhouette)
        {
            ApplyNowSilhouette();
        }
    }

    private void Update()
    {
        if (_applySilhouette)
        {
            ScreenManager.Instance.SetCursorForce(CursorType.None);
        }
    }

    public void ApplyEdSilhouette()
    {
        if (PersistenceManager.Instance)
        {
            var obj = PersistenceManager.Instance.LoadOrCreate<MapEdPersistenceObject>(PERSISTENCE_KEY);
            obj.Silhouette = _silhouette;
        }
    }

    public void ApplyNowSilhouette()
    {
        if (_renderer)
        {
            if (PersistenceManager.Instance)
            {
                var obj = PersistenceManager.Instance.LoadOrCreate<MapEdPersistenceObject>(PERSISTENCE_KEY);
                _renderer.sprite = obj.Silhouette;
            }
        }
    }
}
