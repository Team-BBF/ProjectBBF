using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ProjectBBF/FarmSystem/Farmland/GrownDefinition", fileName = "NewGrownDefinition")]
public class GrownDefinition : ScriptableObject
{
    [System.Serializable]
    public class GrowingSet
    {
        public PlantTile Tile;
        public int NeedStep;
    }

    [SerializeField] private int _continuousCount;
    [SerializeField] private int _continueAndStepIndex;

    [SerializeField] private List<GrowingSet> _needGrowingToNextGrowingStep;
    

    public PlantTile FirstTile => NeedGrowingToNextGrowingStep[0]?.Tile;
    public PlantTile LasTile => NeedGrowingToNextGrowingStep[^1]?.Tile;
    public IReadOnlyList<GrowingSet> NeedGrowingToNextGrowingStep
    {
        get
        {
            if (_needGrowingToNextGrowingStep is null)
            {
                return new List<GrowingSet>();
            }

            return _needGrowingToNextGrowingStep;
        }
    }

    public int ContinuousCount => _continuousCount;

    public int ContinueAndStepIndex
    {
        get
        {
            if (_continueAndStepIndex < 0 || _continueAndStepIndex >= NeedGrowingToNextGrowingStep.Count)
            {
                Debug.LogError($"GrownDefinition({name})의 ContinueAndStepIndex({_continueAndStepIndex}) 값이 잘못 되었습니다." );
                return 0;
            }

            return _continueAndStepIndex;
        }
    }

    public int TotalNeedStep
    {
        get
        {
            if (NeedGrowingToNextGrowingStep.Any() is false) return 0;

            return NeedGrowingToNextGrowingStep[^1].NeedStep;
        }
    }
    
}
