using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DS.Core;
using DS.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueContext
{
    private DialogueRuntimeTree _tree;
    private float _duration;
    private Action<string> _textInput;

    private DialogueController _controller;

    private CancellationTokenSource _source;
    private ProcessorData _processorData;

    public bool CanNext => CurrentNode is not null;

    public DialogueRuntimeNode CurrentNode { get; private set; }

    public bool IsRunning { get; private set; }

    public async UniTask Next()
    {
        if (CanNext == false) return;
        if (IsRunning) return;

        if (_source is null)
        {
            _source = CancellationTokenSource.CreateLinkedTokenSource(
                GlobalCancelation.PlayMode,
                new CancellationToken()
            );
        }

        DialogueItem item = null;
        IsRunning = true;
        try
        {
            begin:

            if (CurrentNode is null)
            {
                IsRunning = false;
                return;
            }

            item = CurrentNode.CreateItem();

            if (item is TextItem textItem)
            {
                _controller.SetPortrait(textItem.ActorKey, textItem.PortraitKey);
                _controller.SetDisplayName(textItem.ActorKey);

                var link = CancellationTokenSource.CreateLinkedTokenSource(
                    GlobalCancelation.PlayMode,
                    _source.Token
                );

                var textTree = TextUtil.CreateTextTree(textItem.Text, _processorData);
                var textTask = TextUtil.DoTextUniTask(_textInput, textTree, _duration, false, _processorData, link.Token);

                await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                    link.Token);

                bool skipped = textTask.Status == UniTaskStatus.Pending;
                link.Cancel();
                _textInput(textTree.ToString());
                CurrentNode = textItem.Node.GetNext();

                if (skipped)
                {
                    await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                        _source.Token);
                }
            }
            else if (item is BranchItem branchItem)
            {
                _controller.SetPortrait(branchItem.ActorKey, branchItem.PortraitKey);
                _controller.SetDisplayName(branchItem.ActorKey);

                var link = CancellationTokenSource.CreateLinkedTokenSource(
                    GlobalCancelation.PlayMode,
                    _source.Token
                );

                var textTree = TextUtil.CreateTextTree(branchItem.Text, _processorData);
                
                await UniTask.WhenAny(
                    TextUtil.DoTextUniTask(_textInput,textTree, _duration, false, _processorData, link.Token),
                    UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                        link.Token
                    )
                );

                link.Cancel();
                _textInput(textTree.ToString());


                var result = await _controller.GetBranchResultAsync
                (
                    branchItem.BranchTexts.Select(x => _controller.GetField<BranchFieldButton>().Init(x))
                        .ToArray<DialogueBranchField>(),
                    _source.Token
                );

                CurrentNode = branchItem.Node.GetNext(result.index);

                goto begin;
            }
            else if (item is ExecutionItem executionItem)
            {
                executionItem.Execution();
                CurrentNode = executionItem.Node.NextNode;

                goto begin;
            }
            else if (item is ConditionItem conditionItem)
            {
                CurrentNode = conditionItem.GetNextNode();
                goto begin;
            }
        }
        catch (OperationCanceledException)
        {
            if (item is TextItem textItem)
            {
                Debug.Log("canceld text");
                _textInput(textItem.Text);
                CurrentNode = textItem.Node.GetNext();
            }
            else if (item is BranchItem branchItem)
            {
                _textInput(branchItem.Text);
                _controller.ResetBranch();
                _controller.SetTextVisible(true);
                CurrentNode = branchItem.Node.GetNext(0);
            }

            _source = null;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        IsRunning = false;
    }

    public void Cancel()
    {
        _source?.Cancel();
        _source = null;
    }

    public DialogueContext(DialogueRuntimeTree tree, float duration, DialogueController controller, ProcessorData processorData)
    {
        _tree = tree;
        _textInput = str =>
        {
            controller.DialogueText = str;
            AudioManager.Instance.PlayOneShot("SFX", "SFX_Dialogue_Text");
        };
        _duration = duration;
        _controller = controller;
        _processorData = processorData;
        _source = new();
        CurrentNode = tree.EntryPoint;
    }
}