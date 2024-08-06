using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    
    [SerializeField] private RectTransform _skipArrow;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private GameObject _content;

    [SerializeField] private float _skipTwinkleDuration = 0.5f;
    [SerializeField] private float _skipTwinkleX = 0.35f;
    [SerializeField] private Ease _skipTwinkleEase = Ease.Unset;

    [SerializeField] private List<Button> _branchButtons;

    public List<Button> BranchButtons => _branchButtons;

    public string DialogueText
    {
        get => _dialogueText.text;
        set => _dialogueText.text = value;
    }

    public bool Visible
    {
        get => _content.activeSelf;
        set => _content.SetActive(value);
    }

    private void Awake()
    {
        SetBranchButtonsVisible(false, 0);
        SetTextVisible(true);
        //StartCoroutine(CoAnimateSkipArrow());
    }

    public async UniTask<int> GetPressedButtonIndexAsync(string[] texts)
    {
        List<UniTask<int>> tasks = new List<UniTask<int>>(texts.Length);
        
        for (int i = 0; i < Mathf.Min(_branchButtons.Count, texts.Length); i++)
        {
            int index = i;

            _branchButtons[i].GetComponentInChildren<TMP_Text>().text = texts[i];
            
            var task = UniTask.Create(async () =>
            {
                await _branchButtons[index].OnClickAsync();
                return index;
            });
            
            tasks.Add(task);
        }

        var completedTask = await UniTask.WhenAny(tasks).WithCancellation(GlobalCancelation.PlayMode);
        int resultIndex = completedTask.result;
        
        return resultIndex;
    }

    public void SetBranchButtonsVisible(bool value, int enableCountIfValueIsTrue)
    {
        int count = value ? Mathf.Min(_branchButtons.Count, enableCountIfValueIsTrue) : _branchButtons.Count;
        
        for (int i = 0; i < count; i++)
        {
            _branchButtons[i].gameObject.SetActive(value);
        }
    }

    public void SetTextVisible(bool value)
    {
        _dialogueText.gameObject.SetActive(value);
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
