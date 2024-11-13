using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using DG.Tweening;
using DS.Runtime;
using TMPro;
using UnityEngine.UI;


#if UNITY_EDITOR
using DS.Core;
using UnityEditor;
#endif

public class DialogueView : MonoBehaviour
{
    [SerializeField] private float _textCompletionDuration = 0.35f;
    
    [SerializeField] private RectTransform _skipArrow;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _displayNameText;
    [SerializeField] private TMP_Text _debugFileNameText;
    [SerializeField] private GameObject _debugFileNameFrame;
    [SerializeField] private Image _portrait;

    [SerializeField] private float _skipTwinkleDuration = 0.5f;
    [SerializeField] private float _skipTwinkleX = 0.35f;
    [SerializeField] private Ease _skipTwinkleEase = Ease.Unset;

    [SerializeField] private Transform _fieldContent;
    [SerializeField] private List<DialogueBranchField> _branchFields;

    private Dictionary<Type, DialogueBranchField> _fieldTable;
    public float TextCompletionDuration => _textCompletionDuration;
    
    public IReadOnlyDictionary<Type, DialogueBranchField> BranchFields => _fieldTable;

    public Transform FieldContent => _fieldContent;

    public string DialogueText
    {
        get => _dialogueText.text;
        set => _dialogueText.text = value;
    }

    public string DebugFileText
    {
        get => _debugFileNameText.text;
        set => _debugFileNameText.text = value;
    }
    public string DisplayName
    {
        get => _displayNameText.text;
        set => _displayNameText.text = value;
    }

    public bool Visible
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    public void SetTextVisible(bool value)
    {
        _dialogueText.gameObject.SetActive(value);
    }

    private void Update()
    {
        #if UNITY_EDITOR
        _debugFileNameFrame.SetActive(EditorPrefs.GetBool("__DEBUG_PRINT_MODE__"));
        #endif
    }

    public void DEBUG_Fouce()
    {
#if UNITY_EDITOR
        string ExtractPath(string input)
        {
            // "path: "의 위치를 찾습니다.
            string keyword = "path: ";
            int startIndex = input.IndexOf(keyword);

            // "path: "가 존재하지 않으면 빈 문자열을 반환합니다.
            if (startIndex == -1)
                return string.Empty;

            // 경로 시작 위치를 "path: " 이후로 이동합니다.
            startIndex += keyword.Length;

            // 경로의 끝은 줄바꿈이나 문자열의 끝으로 간주합니다.
            int endIndex = input.IndexOf('\n', startIndex);
            if (endIndex == -1)
                endIndex = input.Length;

            // 시작과 끝 위치 사이의 문자열(경로)을 추출하여 반환합니다.
            return input.Substring(startIndex, endIndex - startIndex).Trim();
        }
        
        var obj = AssetDatabase.LoadAssetAtPath<DialogueContainer>(ExtractPath(DebugFileText));
        if (obj == false) return;
        
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
#endif
    }

    private void Awake()
    {
        
#if UNITY_EDITOR
        _debugFileNameFrame.SetActive(EditorPrefs.GetBool("__DEBUG_PRINT_MODE__"));
#endif
        
        for (int i = 0; i < _fieldContent.childCount; i++)
        {
            var child = _fieldContent.GetChild(i);
            if (child)
            {
                Destroy(child.gameObject);
            }
        }
        
        SetTextVisible(true);
        

        _fieldTable = new();
        Debug.Assert(_branchFields is not null);

        foreach (var field in _branchFields)
        {
            _fieldTable.Add(field.GetType(), field);
        }

    }

    public void SetPortrait(Sprite sprite)
    {
        _portrait.sprite = sprite;

        _portrait.gameObject.SetActive(sprite is not null);
        _portrait.SetNativeSize();
    }
    
    private IEnumerator CoAnimateSkipArrow()
    {
        float x = _skipArrow.anchoredPosition.x;
        while (true)
        {
            yield return _skipArrow
                    .DOAnchorPosX(x - _skipTwinkleX, _skipTwinkleDuration)
                    .SetEase(_skipTwinkleEase)
                    .WaitForCompletion()
                ;
            yield return _skipArrow
                    .DOAnchorPosX(x, _skipTwinkleDuration)
                    .SetEase(_skipTwinkleEase)
                    .WaitForCompletion()
                ;
        }
    } 
}
