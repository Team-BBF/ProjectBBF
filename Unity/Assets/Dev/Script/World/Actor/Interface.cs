using System.Collections;
using System.Collections.Generic;
using DS.Core;
using DS.Runtime;
using JetBrains.Annotations;
using UnityEngine;

public interface IActorStrategy
{
}

public struct DialogueEvent
{
    public static readonly DialogueEvent Empty = new DialogueEvent()
    {
        Container = null,
        Type = DialogueBranchType.None
    };
    
    private ProcessorData _processorData;
    
    public DialogueContainer Container;
    public DialogueBranchType Type;

    public ProcessorData ProcessorData
    {
        get
        {
            if (_processorData is null)
            {
                _processorData = ProcessorData.Default;
            }

            return _processorData;
        }

        set => _processorData = value;
    }

    public bool IsEmpty => Container == false;
}
public interface IBADialogue : IActorBehaviour
{
    
    /// <summary>
    /// 현재 대화 가능한 DialogueContainer를 반환합니다.
    /// 호출되면 다음으로 반환할 DialogueContainer를 준비합니다.
    /// 가능한 대화가 없으면 DialogueContainer 객체는 null, DialogueStartType은 None 입니다.
    /// </summary>
    /// <returns></returns>
    public DialogueEvent DequeueDialogueEvent(); 
    
    /// <summary>
    /// 현재 대화 가능한 DialogueContainer를 반환합니다.
    /// 가능한 대화가 없으면 DialogueContainer 객체는 null, DialogueStartType은 None 입니다.
    /// </summary>
    /// <returns></returns>
    public DialogueEvent PeekDialogueEvent(); 
}

public interface IBANameKey : IActorBehaviour
{
    public string ActorKey { get; }
}

public interface IBAStateTransfer : IActorBehaviour
{
    public void TranslateState(string stateKey);
}


public abstract class ActorComponent: MonoBehaviour
{
    public abstract void Init(Actor actor);
}