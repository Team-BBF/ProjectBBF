using System.Collections.Generic;
using DS.Core;
using DS.Runtime;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;

public abstract class ActorComFavorability : ActorComponent, IBODialogue
{
    [SerializeField] private DialogueContainer _hittedDialogue;
    [SerializeField] private DialogueContainer _wetDialogue;

    public DialogueContainer HittedDialogue => _hittedDialogue;

    public DialogueContainer WetDialogue => _wetDialogue;

    public CollisionInteraction Interaction { get; private set; }

    protected Actor Owner { get; private set; }
    protected FavorabilityData FavorabilityData { get; private set; }
    
    private ProcessorData _processorData;

    public ProcessorData ProcessorData => _processorData;

    public virtual void Init(Actor actor)
    {
        Owner = actor;
        Interaction = actor.Interaction;
        if (ActorDataManager.Instance.CachedDict.TryGetValue(actor.ActorKey, out var favora) is false)
        {
            Debug.LogError($"유효하지 않은 ActorKey({actor.ActorKey})");
            return;
        }
        
        FavorabilityData = favora;

        // 예약 바인딩 데이터
        var table = new Dictionary<string, string>(new List<KeyValuePair<string, string>>()
            {
                new("player", PersistenceManager.Instance.CurrentMetadata.PlayerName)
            }
        );
        _processorData = new ProcessorData(table);
    }

    public abstract DialogueEvent DequeueDialogueEvent();
    public abstract DialogueEvent PeekDialogueEvent();
}