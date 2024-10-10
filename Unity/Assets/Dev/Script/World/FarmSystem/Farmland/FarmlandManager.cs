using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class FarmlandGrownInfo
{

    public bool Empty => Definition == false;

    public bool CanHarvest
    {
        get
        {
            if (Definition is null) return false;

            return GrownStep >= Definition.TotalNeedStep;
        }
    }

    public PlantTile CurrentTile { get; set; }
    public FertilizerTile FertilizerTile { get; set; }
    public GrownDefinition Definition { get; set; }
    public Vector3Int CellPos { get; private set; }
    public int TotalStep { get; set; }

    public int GrownStep { get; set; }
    public bool IsWet { get; set; }
    public int ContinuousCount { get; set; }

    public FarmlandGrownInfo(Vector3Int cellPos)
    {
        CellPos = cellPos;
        
        Reset();
    }

    public void Reset()
    {
        Definition = null;
        FertilizerTile = null;
        CurrentTile = null;
        TotalStep = 0;
        GrownStep = 0;
        ContinuousCount = 0;
        IsWet = false;
    }
}

public class FarmlandManager : MonoBehaviour
{
    [SerializeField] private FarmlandTileController _controller;
    
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GrowUp(1);
        }
    }

    public void GrowUp(int step = 1)
    {
        var grownInfos = _controller.GetAllGrownInfo();

        foreach (FarmlandGrownInfo info in grownInfos)
        {
            if (info.IsWet is false) continue;
            
            int buffGrowingSpeed = info.FertilizerTile ? info.FertilizerTile.BuffGrowingSpeed : 0;
            
            UpdateGrownState(
                info, 
                step + buffGrowingSpeed
                );
        }
        
        _controller.ResetAllWet();
    }

    public void ResetFarm()
    {
        _controller.ResetAllTile();
    }

    public void UpdateGrownState(FarmlandGrownInfo info, int step)
    {
        var def = info.Definition;
        if (def is null) return;
        
        step = Mathf.Clamp(step, 0, def.NeedGrowingToNextGrowingStep.Count - 1);
        info.TotalStep += step;
        info.GrownStep += step;
        PlantTile nextTile = info.CurrentTile;
        
        for (int i = 0; i < def.NeedGrowingToNextGrowingStep.Count; i++)
        {
            var set = def.NeedGrowingToNextGrowingStep.ElementAtOrDefault(i);
                
            if (set is not null && info.GrownStep >= set.NeedStep)
            {
                nextTile = set.Tile;
            }
        }

        info.CurrentTile = nextTile;
        _controller.RefeshPlantTile(info.CellPos);
    }
}
