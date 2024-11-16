using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TimeHudUI : MonoBehaviour
{
    [Serializable]
    public struct SceneSet
    {
        public SceneName Scene;
        public string Text;
    }
    
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _dayText;

    [SerializeField] private List<SceneSet> _dayScene;
    [SerializeField] private List<SceneSet> _timeScene;

    public bool Visible
    {
        get=> gameObject.activeSelf;
        set=> gameObject.SetActive(value);
    }

    public string TimeText
    {
        get => _timeText.text;
        set=> _timeText.text = value;
    }
    
    public bool OverrideTimeText { get; set; }
    
    private void Awake()
    {
        // TODO: 당장 사용 안 함. 기획 변경
        //StartCoroutine(CoUpdate());
    }

    private IEnumerator CoUpdate()
    {
        GameTime before = new GameTime(-1, -1);
        
        while (true)
        {
            var cur = TimeManager.Instance.GetGameTime();
            int curDay = TimeManager.Instance.SaveData.Day;

            string timeText = null;
            string dayText = null;
            if (SceneLoader.Instance)
            {
                foreach (SceneSet set in _timeScene)
                {
                    if (SceneLoader.Instance.CurrentWorldScene == set.Scene)
                    {
                        timeText = set.Text;
                        break;
                    }
                }
                foreach (SceneSet set in _dayScene)
                {
                    if (SceneLoader.Instance.CurrentWorldScene == set.Scene)
                    {
                        dayText = set.Text;
                        break;
                    }
                }
            }
                
                
            _dayText.text = dayText ?? $"Day {curDay:D3}";
                
            if (OverrideTimeText is false)
            {
                _timeText.text = timeText ?? $"{cur.Hour:D2}:{cur.Min:D2} {cur.TimeOfDay}";
            }
            
            yield return null;
        }
    }
}
