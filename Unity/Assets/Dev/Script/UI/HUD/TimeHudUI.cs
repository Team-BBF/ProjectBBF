using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TimeHudUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private ESOGameTimeEvent _event;

    private void Awake()
    {
        StartCoroutine(CoUpdate());
    }

    private IEnumerator CoUpdate()
    {
        GameTime before = new GameTime(-1, -1);
        
        while (true)
        {
            var cur = TimeManager.Instance.GetGameTime();
            int curDay = TimeManager.Instance.SaveData.Day;
            if (cur != before)
            {
                _text.text = $"Day {curDay:D3}\n{cur.Hour:D2}:{cur.Min:D2} {cur.TimeOfDay}";
                before = cur;
            }

            yield return null;
        }
    }
}
