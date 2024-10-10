using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DS.Core;
using JetBrains.Annotations;
using ProjectBBF.Singleton;
using UnityEngine;
using UnityEngine.EventSystems;

[Flags]
public enum DialogueBranchType : int
{
    None = 0,
    Exit = 1,
    Dialogue = 2,
    Gift = 4,
}

[Singleton(ESingletonType.Global)]
public class DialogueController : MonoBehaviourSingleton<DialogueController>
{
    private DialogueView _view;
    private ActorDataManager _actorDataManager;

    private DialogueContext LastestContext { get; set; }

    private const string VIEW_PATH = "Feature/DialogueView";
    private const string DATA_PATH = "Data/ActorDataTable";

    public bool Visible
    {
        get => _view.Visible;
        set => _view.Visible = value;
    }

    public void SetTextVisible(bool value)
        => _view.SetTextVisible(value);

    public bool SetPortrait(string portraitKey)
    {
        Sprite spr = _actorDataManager.GetPortraitFromKey(portraitKey);
        _view.SetPortrait(spr);

        return spr is not null;
    }

    public bool SetPortrait(string actorKey, string portraitKey)
    {
        if (string.IsNullOrEmpty(actorKey)) return false;
        if (string.IsNullOrEmpty(portraitKey)) return false;
        
        if (_actorDataManager.CachedDict.TryGetValue(actorKey, out var data) &&
            data.PortraitTable.Table.TryGetValue(portraitKey, out var sprite))
        {
            _view.SetPortrait(sprite);
            return true;
        }

        return false;
    }

    public bool SetDisplayName(string actorKey)
    {
        if (string.IsNullOrEmpty(actorKey)) return false;
        
        if (_actorDataManager.CachedDict.TryGetValue(actorKey, out var data))
        {
            _view.DisplayName = data.ActorName;
            return true;
        }

        return false;
    }

    public override void PostInitialize()
    {
        _view = Resources.Load<DialogueView>(VIEW_PATH);
        Debug.Assert(_view is not null);

        _view = Instantiate(_view, transform, true);
        _view.gameObject.SetActive(true);

        _actorDataManager = ActorDataManager.Instance;
        Debug.Assert(_actorDataManager is not null);

        ResetDialogue();
    }

    public override void PostRelease()
    {
        _view = null;
    }

    public void ResetDialogue()
    {
        _view.DialogueText = "";
        _view.Visible = false;

        LastestContext?.Cancel();
        LastestContext = null;
        
        _view.SetPortrait(null);
    }

    public string DialogueText
    {
        get => _view.DialogueText;
        set => _view.DialogueText = value;
    }

    public async UniTask<int> GetBranchResultAsync(params string[] texts)
    {
        SetBranchButtonsVisible(true, texts.Length);
        int index = await _view.GetPressedButtonIndexAsync(texts);
        SetBranchButtonsVisible(false);

        return index;
    }

    public void SetBranchButtonsVisible(bool value, int enableCountIfValueIsTrue = 0)
        => _view.SetBranchButtonsVisible(value, enableCountIfValueIsTrue);

    public DialogueContext CreateContext(DialogueContainer container)
        => CreateContext(DialogueRuntimeTree.Build(container));

    public DialogueContext CreateContext(DialogueRuntimeTree tree)
    {
        // 현재 Context가 누군가에게 점유되어 있으면 null 반환
        if (LastestContext != null && LastestContext.CanNext)
        {
            return null;
        }

        var context = new DialogueContext(
            tree,
            _view.TextCompletionDuration,
            this
        );

        LastestContext = context;
        return context;
    }
    public async UniTask<DialogueBranchType> GetBranchResultAsync(DialogueBranchType type)
    {
        if (type == DialogueBranchType.Dialogue) return DialogueBranchType.Dialogue;
        
        var branches = GetBranchText(type);

        int index = await GetBranchResultAsync(branches.Select(x => x.Item1).ToArray());

        return branches[index].Item2;
    }

    private List<(string, DialogueBranchType)> GetBranchText(DialogueBranchType type)
    {
        List<(string, DialogueBranchType)> texts = new List<(string, DialogueBranchType)>(3);
        
        if ((type & DialogueBranchType.Dialogue) == DialogueBranchType.Dialogue)
        {
            texts.Add(("대화", DialogueBranchType.Dialogue));
        }
        if ((type & DialogueBranchType.Gift) == DialogueBranchType.Gift)
        {
            texts.Add(("선물하기", DialogueBranchType.Gift));
        }
        if ((type & DialogueBranchType.Exit) == DialogueBranchType.Exit)
        {
            texts.Add(("나가기", DialogueBranchType.Exit));
        }
        
        return texts;
    }
}