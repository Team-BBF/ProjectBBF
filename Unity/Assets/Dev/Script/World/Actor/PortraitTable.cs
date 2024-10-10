

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortraitTable
{
    [System.Serializable]
    private class Item
    {
        public string Key;
        public Sprite Portrait;
    }

    [SerializeField] private List<Item> _list = new();

    private Dictionary<string, Sprite> _table;

    public Dictionary<string, Sprite> Table
    {
        get
        {
            if (_table is null)
            {
                _table = new Dictionary<string, Sprite>();
                
                _list.ForEach(x=>_table.Add(x.Key, x.Portrait));
            }

            return _table;
        }
    }
}