using System;
using System.Collections.Generic;
using UnityEngine;

public static class GridEvents
{
    public static event Action<Vector2Int> OnGridCellClicked;
    public static event Action<Vector2Int, int> OnBlastDamage;
    public static event Action<Vector2Int, int> OnRocketDamage;

    public static event Action<Vector2Int> OnObstacleDestroyed;
    public static event Action<Vector2Int, Vector2Int> OnGridItemMoved;

    public static event Action<List<GridItem>> OnItemsDestroyed;
    public static event Action<List<FallData>> OnItemsFall;
    public static event Action<List<NewItemData>> OnNewItemsCreated;
    public static event Action<NewRocketData> OnNewRocketCreated;
    public static event Action<RocketBlastData> OnRocketBlastStarted;

    public static event Action<Vector2Int> OnRocketBlastChainStarted;

    public static event Action OnRocketLineClear;
    public static void TriggerRocketLineClear(){
        OnRocketLineClear?.Invoke();
    }



    public static void TriggerRocketBlastStarted(RocketBlastData rocketBlastData){
        OnRocketBlastStarted?.Invoke(rocketBlastData);
    }
    public static void TriggerRocketBlastChainStarted(Vector2Int pos){
        OnRocketBlastChainStarted?.Invoke(pos);
    }
    public static void TriggerGridCellClicked(Vector2Int gridPos)
    {
        OnGridCellClicked?.Invoke(gridPos);
    }
    public static void TriggerBlastDamage(Vector2Int position, int damageAmount)
    {
        OnBlastDamage?.Invoke(position, damageAmount);
    }

    public static void TriggerRocketDamage(Vector2Int position, int damageAmount)
    {
        OnRocketDamage?.Invoke(position, damageAmount);
    }

    public static void TriggerGridItemMoved(Vector2Int oldPosition, Vector2Int newPosition)
    {
        OnGridItemMoved?.Invoke(oldPosition, newPosition);
    }

    public static void TriggerObstacleDestroyed(Vector2Int position)
    {
        OnObstacleDestroyed?.Invoke(position);
    }
    public static void TriggerItemsDestroyed(List<GridItem> items) => OnItemsDestroyed?.Invoke(items);
    public static void TriggerItemsFall(List<FallData> items) => OnItemsFall?.Invoke(items);
    public static void TriggerNewItemsCreated(List<NewItemData> items) => OnNewItemsCreated?.Invoke(items);

    public static void TriggerNewRocketCreated(NewRocketData newRocketData) => OnNewRocketCreated?.Invoke(newRocketData);
}

public struct FallData
{
    public GridItem GridItem;
    public int FallDistance;
    
    
    public FallData(GridItem gridItem, int fallDistance)
    {
        GridItem = gridItem;
        FallDistance = fallDistance;
    }
}

public struct NewItemData
{
    public GridItem GridItem;
    public int FallDistance;
    public Vector2Int SpawnPosition;
    public int Delay;

    public NewItemData(GridItem gridItem, int fallDistance, Vector2Int spawnPosition,int delay)
    {
        GridItem = gridItem;
        FallDistance = fallDistance;
        SpawnPosition = spawnPosition;
        Delay = delay;
    }
}
public struct NewRocketData
{
    public List<GridItem> GridItems;
    public Vector2Int GridPos;
    public GridItem Rocket;
    public NewRocketData(List<GridItem> gridItems,Vector2Int gridPos,GridItem rocket){
        GridItems = gridItems;
        GridPos = gridPos;
        Rocket = rocket;
    }
}
public struct RocketBlastData
{
    public GridItem originalRocket;
    public Queue<AnimationType> animationQueue;
    public Queue<GridItem> itemQueue;
    public Vector2Int startPos, direction;
    public RocketBlastData(GridItem originalRocket,Vector2Int startPos, Vector2Int direction,Queue<AnimationType> animationQueue,Queue<GridItem> gridItems)
    {
        this.originalRocket = originalRocket;
        this.startPos = startPos;
        this.direction = direction;
        this.animationQueue = animationQueue;
        this.itemQueue =gridItems;
    }
}
