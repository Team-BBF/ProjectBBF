using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum ToolType
{
    Nothing,
    Hoe,
    Fertilizer,
    WaterSpray,
    FishingRod,
    Seed,
    Pickaxe,
}

public enum ToolRank : int
{
    R1 = 1,
    R2,
    R3,
    R4,
    R5,
    
    Min = R1,
    Max = R5,
}

[System.Serializable]
public class ToolRequireSet
{
    [SerializeField] private ToolType _requireToolTypes;
    [SerializeField] private ToolRank _requireToolRank;

    public ToolType RequireToolType => _requireToolTypes;
    public ToolRank RequireToolRank => _requireToolRank;

    public ToolRequireSet(ToolType requireToolTypes, ToolRank requireToolRank)
    {
        _requireToolTypes = requireToolTypes;
        _requireToolRank = requireToolRank;
    }
}

[System.Serializable]
public class ItemTypeInfo
{
    [SerializeField] private ToolRequireSet[] _sets = new ToolRequireSet[0];

    public ToolRequireSet[] Sets => _sets;

    public static readonly ItemTypeInfo Empty = new ItemTypeInfo()
    {
        _sets = new ToolRequireSet[] {new ToolRequireSet(ToolType.Nothing, ToolRank.Max)}
    };

    public ItemTypeInfo(ToolRequireSet[] sets)
    {
        _sets = sets;
    }

    public ItemTypeInfo()
    {
    }
    
    
    public bool Contains(ToolType type, ToolRank? rank = null)
        => ToolTypeUtil.Contains(Sets, type, rank);
    public bool ContainsOverRank(ToolType type, ToolRank rank)
        => ToolTypeUtil.ContainsOverRank(Sets, type, rank);
}

public static class ToolTypeUtil
{
    public static bool Contains(ToolRequireSet set, ToolType type, ToolRank? rank = null)
    {
        if (set.RequireToolType == type)
        {
            if (rank is null) return true;

            return set.RequireToolRank == rank;
        }

        return false;
    }
    public static bool Contains(ToolRequireSet[] sets, ToolType type, ToolRank? rank = null)
    {
        foreach (ToolRequireSet set in sets)
        {
            if (Contains(set, type, rank))
            {
                return true;
            }
        }

        return false;
    }

    public static bool Contains(ToolRequireSet a, ToolRequireSet b)
    {
        if (Contains(a, b.RequireToolType, b.RequireToolRank))
        {
            return true;
        }

        return false;
    }
    public static bool Contains(ToolRequireSet[] setsA, ToolRequireSet[] setsB)
    {
        foreach (ToolRequireSet a in setsA)
        {
            foreach (ToolRequireSet b in setsB)
            {
                if (Contains(a, b))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool ContainsOverRank(ToolRequireSet[] setsA, ToolRequireSet[] setsB)
    {
        foreach (ToolRequireSet a in setsA)
        {
            foreach (ToolRequireSet b in setsB)
            {
                if (ContainsOverRank(a, b.RequireToolType, b.RequireToolRank))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool ContainsOverRank(ToolRequireSet set, ToolType type, ToolRank rank)
    {
        if (set.RequireToolType == type)
        {
            return (int)set.RequireToolRank >= (int)rank;
        }

        return false;
    }
    public static bool ContainsOverRank(ToolRequireSet[] sets, ToolType type, ToolRank rank)
    {
        foreach (ToolRequireSet set in sets)
        {
            if (ContainsOverRank(set, type, rank))
            {
                return true;
            }
        }

        return false;
    }
}