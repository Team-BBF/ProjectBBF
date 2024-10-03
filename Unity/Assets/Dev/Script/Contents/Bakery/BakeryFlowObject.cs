using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.Serialization;

public class BakeryFlowObject : MonoBehaviour, IObjectBehaviour
{
    [field: SerializeField, AutoProperty, MustBeAssigned, InitializationField]
    private FadeinoutObject _fadeObject;

    public event Action<BakeryFlowObject, CollisionInteractionMono> OnActivate;
    public event Action<BakeryFlowObject, CollisionInteractionMono> OnInteraction;
    
    public event Action<BakeryFlowObject, CollisionInteractionMono> OnEnter;
    public event Action<BakeryFlowObject, CollisionInteractionMono> OnExit;
    public CollisionInteraction Interaction => _fadeObject.Interaction;

    public FadeinoutObject FadeObject => _fadeObject;

    private void Awake()
    {
        var info = ObjectContractInfo.Create(() => gameObject);
        _fadeObject.Interaction.SetContractInfo(info, this);

        info.AddBehaivour<BakeryFlowObject>(this);

        _fadeObject.OnEnter += x =>
        {
            OnEnter?.Invoke(this, x);
            StartCoroutine(CoUpdate(x));
        };

        _fadeObject.OnExit += x =>
        {
            OnExit?.Invoke(this, x);
            StopAllCoroutines();
        };
    }

    private IEnumerator CoUpdate(CollisionInteractionMono activator)
    {
        while (true)
        {
            if (InputManager.Map.Minigame.BakeryKeyPressed.triggered)
            {
                OnActivate?.Invoke(this, activator);
            }

            if (InputManager.Map.Player.Interaction.triggered)
            {
                OnInteraction?.Invoke(this, activator);
            }

            yield return null;
        }
    }
}

public abstract class BakeryFlowBehaviour : MonoBehaviour
{
    [field: SerializeField, AutoProperty, MustBeAssigned, InitializationField]
    private BakeryFlowObject _flowObject;

    public CollisionInteraction Interaction => _flowObject.Interaction;
    public BakeryFlowObject FlowObject => _flowObject;

    public FadeinoutObject FadeObject => FlowObject.FadeObject;

    protected virtual void Awake()
    {
        _flowObject.OnActivate += OnActivate;
        _flowObject.OnInteraction += OnInteraction;
    }

    protected virtual void OnDestroy()
    {
        if (FadeObject)
        {
            _flowObject.OnActivate -= OnActivate;
            _flowObject.OnInteraction -= OnInteraction;
        }
    }

    protected abstract void OnActivate(BakeryFlowObject flowObject, CollisionInteractionMono activator);
    protected abstract void OnInteraction(BakeryFlowObject flowObject, CollisionInteractionMono activator);
    protected abstract void OnEnter(BakeryFlowObject flowObject, CollisionInteractionMono activator);
    protected abstract void OnExit(BakeryFlowObject flowObject, CollisionInteractionMono activator);
}

public abstract class BakeryFlowBehaviourBucket : BakeryFlowBehaviour
{
    [Serializable]
    public enum Resolvor
    {
        Dough,
        Additive,
        Baking,
    }
    
    [SerializeField] private Resolvor _resolvor;
    [SerializeField] private List<ItemBucket> _buckets;

    private int _currentBucketIndex;

    public int BucketLength => _buckets.Count;
    public bool IsFullBucket => _currentBucketIndex >= BucketLength;

    public List<ItemData> StoredItems => _buckets
        .Where(x => x)
        .Select(x => x.Item)
        .ToList();

    protected override void Awake()
    {
        base.Awake();
        _buckets.ForEach(y => y.OnFade(0f));

        FadeObject.OnFadeAlpha += OnBucketFade;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (FadeObject)
        {
            FadeObject.OnFadeAlpha -= OnBucketFade;
        }
    }

    protected override void OnInteraction(BakeryFlowObject flowObject, CollisionInteractionMono activator)
    {
        if (activator.Owner is not PlayerController pc) return;

        IInventorySlot slot = pc.Inventory.CurrentItemSlot;

        int index = _currentBucketIndex - 1;
        if(!(index < 0 || index >= BucketLength) && _buckets[index].Item )
        {
            if (slot.Empty)
            {
                if (SlotChecker.Contains(slot.TrySet(_buckets[index].Item, 1), SlotStatus.Success) is false)
                {
                    Debug.LogError("정의되지 않은 동작");
                }

                _buckets[index].Item = null;
                _currentBucketIndex = index;

                return;
            }
            if(_buckets[index].Item == slot.Data)
            {
                if (SlotChecker.Contains(slot.TryAdd(1), SlotStatus.Success))
                {
                    _buckets[index].Item = null;
                    _currentBucketIndex = index;

                    return;
                }

            }
        }

        ItemData currentItem = slot.Data;
        if (currentItem == false) return;
        if (SetBucket(_currentBucketIndex, currentItem))
        {
            if (SlotChecker.Contains(slot.TryAdd(-1, true), SlotStatus.Success) is false)
            {
                SetBucket(_currentBucketIndex, null);
                return;
            }

            _currentBucketIndex++;
        }
    }

    public bool SetBucket(int index, ItemData itemData)
    {
        if (index < 0 || index >= BucketLength) return false;
        if (itemData && CanStore(index, itemData) is false) return false;

        _buckets[index].Item = itemData;
        return true;
    }

    public void ClearBucket()
    {
        _currentBucketIndex = 0;
        foreach (ItemBucket b in _buckets)
        {
            b.Item = null;
        }
    }

    public ItemData GetBucket(int index)
    {
        if (index < 0 || index >= BucketLength) return null;
        return _buckets[index].Item;
    }


    protected bool CanStore(int index, ItemData itemData)
    {
        var resolver = BakeryRecipeResolver.Instance;
        
        switch (_resolvor)
        {
            case Resolvor.Dough:
                return resolver.CanListOnDoughIngredient(itemData);
            case Resolvor.Baking:
                return resolver.CanListOnDough(itemData);
            case Resolvor.Additive:
                if (index == 1)
                {
                    return resolver.CanListOnAdditive(itemData);
                }

                return resolver.CanListOnBakedBread(itemData);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnBucketFade(float alpha)
    {
        foreach (ItemBucket bucket in _buckets)
        {
            bucket.OnFade(alpha);
        }
    }
    

    protected (ItemData failItem, ItemData resultItem, float duration) GetResolvedItem()
    {
        var resolver = BakeryRecipeResolver.Instance;
        ItemData failItem;
        ItemData resultItem = null;
        float duration = 0f;
        var bucketItems = StoredItems;

        if (bucketItems.Count is 0)
        {
            Debug.LogError("Bucket의 수가 0입니다.");
            return (resolver.FailDoughRecipe.DoughItem, resolver.FailDoughRecipe.DoughItem, 0f);
        }

        switch (_resolvor)
        {
            case Resolvor.Dough:
                failItem = resolver.FailDoughRecipe.DoughItem;
                duration = resolver.FailDoughRecipe.KneadDuration;
                var doughRecipe = resolver.ResolveDough(bucketItems);

                if (doughRecipe)
                {
                    resultItem = doughRecipe.DoughItem;
                    duration = doughRecipe.KneadDuration;
                }
                break;
            case Resolvor.Baking:
                failItem = resolver.FailBakedBreadRecipe.BreadItem;
                duration = resolver.FailBakedBreadRecipe.MinigameBarDuration;
                var bakingRecipe = resolver.ResolveBakedBread(bucketItems[0]);

                if (bakingRecipe)
                {
                    resultItem = bakingRecipe.BreadItem;
                    duration = bakingRecipe.MinigameBarDuration;
                }
                break;
            case Resolvor.Additive:
                failItem = resolver.FailResultBreadRecipe.ResultItem;
                duration = resolver.FailResultBreadRecipe.CompletionDuration;
                var additiveRecipe = resolver.ResolveAdditive(bucketItems[0], bucketItems.GetRange(1, bucketItems.Count - 1));

                if (additiveRecipe)
                {
                    resultItem = additiveRecipe.ResultItem;
                    duration = additiveRecipe.CompletionDuration;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return (failItem, resultItem, duration);
    }
}