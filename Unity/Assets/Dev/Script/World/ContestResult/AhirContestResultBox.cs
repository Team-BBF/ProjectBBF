using System.Collections;
using System.Collections.Generic;
using DS.Runtime;
using ProjectBBF.Event;
using ProjectBBF.Persistence;
using UnityEngine;

public class AhirContestResultBox : BakeryFlowBehaviourBucket
{
    [SerializeField] private bool _passItemToNextCh;
    [SerializeField] private string doOnceKey;
    [SerializeField] private string _ahirBoxKey;
    [SerializeField] private Animator _ani;

    public static ItemData ResultItem { get; private set; }
    
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        ResultItem = null;
    }

    protected override void Awake()
    {
        base.Awake();

        if (_ani == false)
        {
            _ani = GetComponentInChildren<Animator>();
        }
        
        
        var ahirBox = PersistenceManager.Instance.LoadOrCreate<AhirBoxPersistenceObject>(_ahirBoxKey);

        PersistenceManager.Instance.OnGameDataLoaded += OnLoaded;

        if (string.IsNullOrEmpty(ahirBox.ItemKey) is false)
        {
            var resolver = DataManager.Instance.GetResolver<IItemDataResolver>();

            if (resolver.TryGetData(ahirBox.ItemKey, out var itemData))
            {
                SetBucket(0, itemData);
                TEMP_BucketIndex = 1;
            }
        }
    }

    private void OnLoaded(PersistenceManager obj)
    {
        if (this == false)
        {
            obj.OnGameDataLoaded -= OnLoaded;
            return;
        }
        
        var ahirBox = obj.LoadOrCreate<AhirBoxPersistenceObject>(_ahirBoxKey);
        var resolver = DataManager.Instance.GetResolver<IItemDataResolver>();

        if (resolver.TryGetData(ahirBox.ItemKey, out var itemData))
        {
            SetBucket(0, itemData);
            TEMP_BucketIndex = 1;
        }
    }

    protected override bool CanStore(int index, ItemData itemData)
    {
        if (index != 0) return false;

        var inst = BakeryRecipeResolver.Instance;
        
        return inst.CanListOnBakedBread(itemData) || 
               inst.CanListOnCompletionBread(itemData) ||
               inst.FailBakedBreadRecipe.BreadItem == itemData ||
               inst.FailResultBreadRecipe.ResultItem == itemData;
    }

    protected override void OnChangedBuket(int index, ItemData itemData)
    {
        base.OnChangedBuket(index, itemData);
        ResultItem = itemData;
        
        var ahirBox = PersistenceManager.Instance.LoadOrCreate<AhirBoxPersistenceObject>(_ahirBoxKey);

        if (itemData)
        {
            ahirBox.ItemKey = itemData.ItemKey;
            if (_passItemToNextCh)
            {
                ahirBox.PassItemKey = itemData.ItemKey;
            }
        }
        else
        {
            ahirBox.ItemKey = "";
            if (_passItemToNextCh)
            {
                ahirBox.PassItemKey = "";
            }
        }
        
        var blackboard = PersistenceManager.Instance.LoadOrCreate<DoOnceHandlerPersistenceObject>("DoOnce");
        if (itemData)
        {
            if (blackboard.DoOnceList.Contains(doOnceKey) is false)
            {
                blackboard.DoOnceList.Add(doOnceKey);
            }
        }
        else
        {
            blackboard.DoOnceList.Remove(doOnceKey);
        }
    }

    protected override void OnActivate(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        
    }

    protected override void OnEnter(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (_ani)
        {
            _ani.SetTrigger("Open");
        }
    }

    protected override void OnExit(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (_ani)
        {
            _ani.SetTrigger("Close");
        }
    }
}
