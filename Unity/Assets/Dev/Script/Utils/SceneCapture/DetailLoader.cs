using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using MyBox;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

public class DetailLoader : MonoBehaviour
{
    [Serializable]
    public class Item1
    {
        public List<Item2> List = new List<Item2>();
    }

    [Serializable]
    public class Item2
    {
        public SpriteRenderer Renderer;
        public int X;
        public int Y;
    }

    private string _detailsPath;
    private string _mapsPath;
    
    private string _mapName;

    private Vector2Int _resolution;
    private float _ppu;
    private Vector3 _offset;
    private Vector2Int _iteration;
    private int _sortingLayer;
    private int _order;
     private Transform _content;

     public Transform Content
     {
         get => _content;
         set => _content = value;
     }

     public string DetailsPath
    {
        get => _detailsPath;
        set => _detailsPath = value;
    }

    public string MapName
    {
        get => _mapName;
        set => _mapName = value;
    }

    public Vector2Int Resolution
    {
        get => _resolution;
        set => _resolution = value;
    }

    public float PPU
    {
        get => _ppu;
        set => _ppu = value;
    }

    public Vector3 Offset
    {
        get => _offset;
        set => _offset = value;
    }

    public Vector2Int Iteration
    {
        get => _iteration;
        set => _iteration = value;
    }

    public int SortingLayer
    {
        get => _sortingLayer;
        set => _sortingLayer = value;
    }

    public string MapsPath
    {
        get => _mapsPath;
        set => _mapsPath = value;
    }

    public int Order
    {
        get => _order;
        set => _order = value;
    }

    [field: SerializeField, HideInInspector] private List<Item1> _rendererList;

    public float Unit => SceneCaptureUtility.CalculateUnit(_resolution.x, _ppu);
    
    public void LoadDetails()
    {
        transform.position = Vector3.zero;

        var sprites = LoadSprites();
        var doubleList = SortSprites(sprites);

        InitDetailRenderer(doubleList);
        
        Debug.Log("디테일 로드 성공");
    }

    private void InitDetailRenderer(List<List<Sprite>> list)
    {
        if (_rendererList != null)
        {
            _rendererList.ForEach(x => x.List.ForEach(y =>
            {
                if (y.Renderer)
                {
                    DestroyImmediate(y.Renderer.gameObject);
                }
            }));
        }

        _rendererList = new List<Item1>();

        foreach (var inList in list)
        {
            var rendererList = new Item1();
            _rendererList.Add(rendererList);

            foreach (var sprite in inList)
            {
                var matchCollection = Regex.Matches(sprite.name, @"\d+");
                if (matchCollection.Count != 2) continue;

                int y = Convert.ToInt32(matchCollection[1].Value);
                int x = Convert.ToInt32(matchCollection[0].Value);

                var renderer = new GameObject(sprite.name).AddComponent<SpriteRenderer>();
                renderer.sortingOrder = _order;
                renderer.sortingLayerID = _sortingLayer;
                renderer.transform.SetParent(_content);
                renderer.sprite = sprite;
                rendererList.List.Add(new Item2()
                {
                    Renderer = renderer,
                    X = x,
                    Y = y
                });
                EditorUtility.SetDirty(this);


               //renderer.transform.position = 
               //    new Vector3(
               //        _resolution.x* x, 
               //        -_resolution.y * y, 0f) * Unit * 2f + _offset +  new Vector3(_resolution.x, -_resolution.y, 0f)
               //    * Unit;
               renderer.transform.position = new Vector3(x, -y) * Unit + _offset +  new Vector3(Unit, -Unit, 0f) * 0.5f;
            }
        }
    }

    private List<List<Sprite>> SortSprites(List<Sprite> list)
    {
        OrderedDictionary dict = new OrderedDictionary();

        foreach (var sprite in list)
        {
            var matchCollection = Regex.Matches(sprite.name, @"\d+");
            if (matchCollection.Count != 2) continue;

            int y = Convert.ToInt32(matchCollection[1].Value);
            int x = Convert.ToInt32(matchCollection[0].Value);

            if (dict.Contains(y) == false)
            {
                var targetList = new List<(int, Sprite)>();
                dict.Add(y, targetList);
                targetList.Add((x, sprite));
            }
            else
            {
                var targetList = dict[y] as List<(int, Sprite)>;
                targetList.Add((x, sprite));
            }
        }

        foreach (List<(int, Sprite)> targetList in dict.Values)
        {
            targetList.Sort((x, y) =>
            {
                if (x.Item1 > y.Item1) return -1;
                if (x.Item1 < y.Item1) return 1;

                return 0;
            });
        }

        var newList = new List<List<Sprite>>();

        foreach (List<(int, Sprite)> targetList in dict.Values)
        {
            newList.Add(targetList.Select(x => x.Item2).ToList());
        }

        return newList;
    }

    private List<Sprite> LoadSprites()
    {
        List<Sprite> sprites = new List<Sprite>(10);

        for (int i = 0; i < _iteration.y; i++)
        {
            for (int j = 0; j < _iteration.x; j++)
            {
                var spr = AssetDatabase.LoadAssetAtPath<Sprite>(_detailsPath + $"/Detail_{_mapName}_{j}_{i}.png");
                if (spr is null) continue;

                spr.name = $"Detail_{j}_{i}";
                sprites.Add(spr);
            }
        }

        return sprites;
    }
}
#endif