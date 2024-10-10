using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProjectBBF.Event;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class FarmlandTileController : MonoBehaviour, 
    IBODestoryTile, IBOCultivateTile, IBOPlantTile, IBOFertilizerTile, IBOSprinkleWaterTile, IBOCollectPlant
{
    [SerializeField] protected CollisionInteraction _interaction;
    [SerializeField] protected Tilemap _platformTilemap;
    [SerializeField] protected Tilemap _wetTilemap;
    [SerializeField] protected Tilemap _fertilizerTilemap;
    [SerializeField] protected Tilemap _plantTilemap;
    [SerializeField] protected CultivationTile _defaultCultivationTile;
    [SerializeField] protected CultivationTile _wetTile;

    private Dictionary<Vector3Int, IFarmlandTile> _originTable = new();
    private Dictionary<Vector3Int, FarmlandGrownInfo> _grownInfos = new();

    private void Awake()
    {
        Init();

        var info = ObjectContractInfo.Create(() => gameObject);
        _interaction.SetContractInfo(info, this);

        info.AddBehaivour<IBODestoryTile>(this);
        info.AddBehaivour<IBOCultivateTile>(this);
        info.AddBehaivour<IBOPlantTile>(this);
        info.AddBehaivour<IBOFertilizerTile>(this);
        info.AddBehaivour<IBOSprinkleWaterTile>(this);
        info.AddBehaivour<IBOCollectPlant>(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ResetAllTile();
        }
    }

    private void Init()
    {
        BoundsInt bounds = _platformTilemap.cellBounds;
        for (int i = bounds.x; i < bounds.xMax; i++)
        {
            for (int j = bounds.y; j < bounds.yMax; j++)
            {
                for (int k = bounds.z; k < bounds.zMax; k++)
                {
                    var cellPos = new Vector3Int(i, j, k);
                    
                    var tile = _platformTilemap.GetTile(cellPos);                    
                    
                    if (tile is not IFarmlandTile farmlandTile) continue;
                    
                    _grownInfos.Add(cellPos, new FarmlandGrownInfo(cellPos));
                    _originTable[cellPos] = farmlandTile;
                }

            }
        }
    }
    
    public bool TryCultivateTile(Vector3Int cellPos, CultivationTile tile)
    {
        if (CanCultivate(cellPos) == false) return false;

        if (tile is null)
        {
            tile = _defaultCultivationTile;
        }
        
        SetTile(cellPos, tile);
        _platformTilemap.RefreshAllTiles();
        return true;
    }

    [CanBeNull]
    public FarmlandGrownInfo GetGrownInfo(Vector3Int cellPos)
    {
        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            return info;
        }

        return null;
    }

    public List<FarmlandGrownInfo> GetAllGrownInfo()
        => _grownInfos.Values.ToList();

    public bool DestroyPlantTile(Vector3Int cellPos, ItemTypeInfo itemTypeInfo, List<ItemData> list)
    {
        var tile = _plantTilemap.GetTile<PlantTile>(cellPos);

        if (tile is null) return false;
        
        if (ToolTypeUtil.Contains(tile.RequireTools, itemTypeInfo.Sets) is false)
        {
            return false;
        }

        for (int i = 0; i < tile.DropItemCount; i++)
        {
            // 필드에 아이템 드랍되게 하면 이 코드 사용
            //var r = FieldItem.Create(new FieldItem.FieldItemInitParameter()
            //{
            //    ItemData = tile.DropItem,
            //    Position = _plantTilemap.CellToWorld(cellPos)
            //});

            if (list is not null)
            {
                list.Add(tile.DropItem);
            }
        }
        
        ResetPlantTile(cellPos);
        ResetFertilizerTile(cellPos);

        return true;
    }

    public bool DestroyCultivationTile(Vector3Int cellPos, ItemTypeInfo itemTypeInfo, List<ItemData> list)
    {
        var tile = _platformTilemap.GetTile<CultivationTile>(cellPos);

        if (tile is null) return false;

        if (ToolTypeUtil.Contains(tile.RequireTools, itemTypeInfo.Sets) is false)
        {
            return false;
        }

        for (int i = 0; i < tile.DropItemCount; i++)
        {
            // 필드에 아이템 드랍되게 하면 이 코드 사용
            //var r = FieldItem.Create(new FieldItem.FieldItemInitParameter()
            //{
            //    ItemData = tile.DropItem,
            //    Position = _plantTilemap.CellToWorld(cellPos)
            //});

            if (list is not null && tile.DropItem is not null)
            {
                list.Add(tile.DropItem);
            }
        }
        
        ResetPlatformTile(cellPos);
        ResetFertilizerTile(cellPos);
        ResetWet(cellPos);
        return true;
    }
    
    public void ResetPlantTile(Vector3Int cellPos)
    {
        _plantTilemap.SetTile(cellPos, null);
        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            info.Definition = null;
            info.CurrentTile = null;
            info.GrownStep = 0;
            info.TotalStep = 0;
            info.ContinuousCount = 0;
        }
    }
    public void ResetPlatformTile(Vector3Int cellPos)
    {
        if (_originTable.TryGetValue(cellPos, out var tile))
        {
            _platformTilemap.SetTile(cellPos, tile as TileBase);
        }
        else
        {
            _platformTilemap.SetTile(cellPos, null);
        }
    }
    public void ResetFertilizerTile(Vector3Int cellPos)
    {
        _fertilizerTilemap.SetTile(cellPos, null);
        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            info.FertilizerTile = null;
        }
    }

    public void ResetWet(Vector3Int cellPos)
    {
        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            info.IsWet = false;
            _wetTilemap.SetTile(cellPos, null);
        }
    }
    public void ResetAllWet()
    {
        _wetTilemap.ClearAllTiles();
        foreach (var pair in _grownInfos)
        {
            pair.Value.IsWet = false;
        }
        
    }

    public void ResetAllTile()
    {
        _platformTilemap.ClearAllTiles();
        _plantTilemap.ClearAllTiles();
        _fertilizerTilemap.ClearAllTiles();
        _wetTilemap.ClearAllTiles();

        foreach (FarmlandGrownInfo info in _grownInfos.Values)
        {
            info.Reset();
        }
        
        foreach (var pair in _originTable)
        {
            Vector3Int cellPos = pair.Key;
            IFarmlandTile tile = pair.Value;
            
            _platformTilemap.SetTile(cellPos, tile as TileBase);
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPos) => _platformTilemap.WorldToCell(worldPos);

    public bool CanCultivate(Vector3Int cellPos)
    {
        var tile = _platformTilemap.GetTile<PlatformTile>(cellPos);
        return tile is not null;
    }

    public bool CanPlant(Vector3Int cellPos)
    {
        var plantTile = _plantTilemap.GetTile<PlantTile>(cellPos);
        var cultivationTile = _platformTilemap.GetTile<CultivationTile>(cellPos);
        
        return plantTile is null && cultivationTile is not null;
    }

    public bool CanPlantFertilizer(Vector3Int cellPos)
    {
        if (_platformTilemap.GetTile<CultivationTile>(cellPos) == false) return false;
        
        var tile = _fertilizerTilemap.GetTile<FertilizerTile>(cellPos);
        return tile is null;
    }

    public void RefeshPlantTile(Vector3Int cellPos)
    {
        var info = GetGrownInfo(cellPos);
        if (info is null) return;

        SetTile(cellPos, info.CurrentTile);
    }
    
    public bool Plant(Vector3Int cellPos, GrownDefinition definition)
    {
        if (CanPlant(cellPos) == false) return false;
        
        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            info.Definition = definition;
            SetTile(cellPos, definition.FirstTile);

            return true;
        }

        return false;
    }

    public bool PlantFertilizer(Vector3Int cellPos, FertilizerTile fertilizerTile)
    {
        if (CanPlantFertilizer(cellPos) is false) return false;
        
        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            info.FertilizerTile = fertilizerTile;
            SetTile(cellPos, fertilizerTile);

            return true;
        }
        
        return false;
    }

    public bool SprinkleWater(Vector3Int cellPos)
    {
        var tile = _platformTilemap.GetTile<CultivationTile>(cellPos);
        if (tile is null) return false;

        if (_grownInfos.TryGetValue(cellPos, out var info))
        {
            var color = Color.white * 0.75f;
            color.a = 1f;
            _wetTilemap.SetTile(cellPos, _wetTile);
            _wetTilemap.SetColor(cellPos, color);
            info.IsWet = true;
            return true;
        }

        return false;
    }
    
    private void SetTile(Vector3Int cellPos, IFarmlandTile tile)
    {
        Tilemap tilemap = null;
        
        switch (tile)
        {
            case PlantTile:
                tilemap = _plantTilemap;
                break;
            case CultivationTile:
                tilemap = _platformTilemap;
                break;
            default:
                tilemap = _fertilizerTilemap;
                break;
        }
        
        Debug.Assert(tilemap is not null);
        
        tilemap.SetTile(cellPos, tile as TileBase);

    }

    public CollisionInteraction Interaction => _interaction;
    public bool Collect(Vector3 worldPos, List<ItemData> itemList)
    {
        var cellPos = WorldToCell(worldPos);

        if (_grownInfos.TryGetValue(cellPos, out FarmlandGrownInfo info))
        {
            if (info.CanHarvest is false) return false;
            if (info.Definition is null) return false;
            
            info.GrownStep = 0;
            info.CurrentTile = info.Definition.FirstTile;
            info.ContinuousCount += 1;

            if (info.Definition.LasTile is not null && info.Definition.LasTile.DropItem)
            {
                for (int i = 0; i < info.Definition.LasTile.DropItemCount; i++)
                {
                    itemList.Add(info.Definition.LasTile.DropItem);
                }
            }
            

            if (info.ContinuousCount > info.Definition.ContinuousCount)
            {
                info.Reset();
            }
            else
            {
                int index = info.Definition.ContinueAndStepIndex;
                GrownDefinition.GrowingSet set = info.Definition.NeedGrowingToNextGrowingStep[index];

                info.CurrentTile = set.Tile;
                info.GrownStep = set.NeedStep;
            }
            
            _plantTilemap.SetTile(cellPos, info.CurrentTile);
            SetTile(cellPos, info.CurrentTile);
            
            ResetFertilizerTile(cellPos);

            return true;
        }

        return false;
    }

    public bool TryCultivateTile(Vector3 worldPos, CultivationTile tile)
        => TryCultivateTile(WorldToCell(worldPos), tile);

    public bool Plant(Vector3 worldPos, GrownDefinition definition)
        => Plant(WorldToCell(worldPos), definition);

    public bool PlantFertilizer(Vector3 worldPos, FertilizerTile tile)
        => PlantFertilizer(WorldToCell(worldPos), tile);

    public bool SprinkleWater(Vector3 worldPos)
        => SprinkleWater(WorldToCell(worldPos));
    
    /// 
    [CanBeNull]
    public List<ItemData> Destory(Vector3 worldPos, ItemTypeInfo itemTypeInfo)
    {
        IFarmlandTile tile = null;
        Vector3Int cellPos = Vector3Int.zero;
        List<ItemData> list = new List<ItemData>(2);

        bool doAnything = false;
        
        
        // 식물(plant) 파괴 및 아이템 드랍 처리
        cellPos = _plantTilemap.WorldToCell(worldPos);
        tile = _plantTilemap.GetTile<PlantTile>(cellPos);
        if (tile is not null)
        {
            doAnything |= DestroyPlantTile(cellPos, itemTypeInfo, list);
        }
        
        
        // 플렛폼 타일 처리
        cellPos = _platformTilemap.WorldToCell(worldPos);
        tile = _platformTilemap.GetTile<CultivationTile>(cellPos);
        if (tile is not null)
        {
            doAnything |= DestroyCultivationTile(cellPos, itemTypeInfo, list);
        }

        
        return doAnything ? list : null;
    }
}
