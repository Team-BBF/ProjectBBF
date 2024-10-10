using System;
using System.Collections.Generic;
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

    //TODO: 향후 삭제 바람
    //private NpcController _controller;

    private DialogueController _controller;

    private CancellationTokenSource _source;

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
                _controller.SetPortrait(textItem.PortraitKey);
                _controller.SetDisplayName(textItem.ActorKey);
                
               var link = CancellationTokenSource.CreateLinkedTokenSource(
                   GlobalCancelation.PlayMode,
                   _source.Token
               );

               var textTask = TextUtil.DoTextUniTask(_textInput, textItem.Text, _duration, true, link.Token);

               await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                   link.Token);

               bool skipped = textTask.Status == UniTaskStatus.Pending;
               link.Cancel();
                _textInput(textItem.Text);
                CurrentNode = textItem.Node.GetNext();

                if (CurrentNode is null && skipped)
                {
                    await UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update,
                        _source.Token);
                }

                // TODO: 이 코드 왜 추가했었더라?
                //if (CurrentNode is ExecutionRuntimeNode or ConditionRuntimeNode)
                //{
                //    goto begin;
                //}
            }
            else if (item is BranchItem branchItem)
            {
                _controller.SetPortrait(branchItem.PortraitKey);
                _controller.SetDisplayName(branchItem.ActorKey);
                
                var link = CancellationTokenSource.CreateLinkedTokenSource(
                    GlobalCancelation.PlayMode,
                    _source.Token
                );
               
                await UniTask.WhenAny(
                    TextUtil.DoTextUniTask(_textInput, branchItem.Text, _duration, true, link.Token),
                    UniTask.WaitUntil(() => InputManager.Map.UI.DialogueSkip.triggered, PlayerLoopTiming.Update, link.Token
                    )
                );
               
                link.Cancel();
                _textInput(branchItem.Text);
                
                _controller.SetBranchButtonsVisible(true, branchItem.BranchTexts.Length);
                int index = await _controller.GetBranchResultAsync(branchItem.BranchTexts);
                _controller.SetBranchButtonsVisible(false, 0);
                
                CurrentNode = branchItem.Node.GetNext(index);

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
                _controller.SetBranchButtonsVisible(false, 0);
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

    public DialogueContext(DialogueRuntimeTree tree, float duration, DialogueController controller)
    {
        _tree = tree;
        _textInput = str =>controller.DialogueText = str;
        _duration = duration;
        _controller = controller;
        _source = new();
        CurrentNode = tree.EntryPoint;
    }
}