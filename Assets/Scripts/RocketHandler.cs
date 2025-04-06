using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class RocketHandler
{
    // Tracks active rockets in each row (horizontal) and column (vertical)
    public static int CurActiveRockets = 0;
    public static void HandleSingleRocket(GridState gridState, Vector2Int pos, bool directlyCalled = false)
    {
        if (!directlyCalled)
        {
            HandleSingleRocketHelper(gridState, pos);
            return;
        }

        Rocket rocket = gridState.Get(pos) as Rocket;
        if (rocket == null) return;

        var adjacentPositions = gridState.GetAdjacentPositions(pos)
        .Where(p => gridState.IsValid(p))
        .ToList();

        var adjacentRockets = adjacentPositions
            .Where(p => gridState.Get(p) != null && gridState.Get(p).IsSpecialItem())
            .Select(p => gridState.Get(p) as SpecialItem)
            .Where(item => item != null && item.IsRocket())
            .ToList();

        if (adjacentRockets.Count >= 1)
        {
            HandleRocketCombo(gridState, pos);

        }
        else
        {
            HandleSingleRocketHelper(gridState, pos);

        }
    }

    public static void HandleRocketCombo(GridState gridState, Vector2Int center)
    {
        List<GridItem> itemsToAnimate = new List<GridItem>();

        List<Vector2Int> surroundingPositions = gridState.GetArea(center);


        itemsToAnimate.Add(gridState.Get(center));

        gridState.Set(center, null);
        foreach (var pos in surroundingPositions)
        {
            if (gridState.IsValid(pos) && gridState.Get(pos) != null)
            {
                GridItem item = gridState.Get(pos);

                itemsToAnimate.Add(item);

                gridState.Set(pos, null);
            }
        }
        List<RocketBlastData> rocketBlastDatas = new List<RocketBlastData>();
        int rocketCount =0;
        for (int i = -1; i < 2; i++)
        {
            Vector2Int pos1 = center + new Vector2Int(0, i);
            if (gridState.IsValid(pos1))
            {
                rocketCount+=2;
                rocketBlastDatas.Add(ProcessRocketMovement(null, gridState, pos1, Vector2Int.left));
                rocketBlastDatas.Add(ProcessRocketMovement(null, gridState, pos1, Vector2Int.right));
            }
            Vector2Int pos2 = center + new Vector2Int(i, 0);
            if (gridState.IsValid(pos2))
            {
                rocketCount+=2;
                rocketBlastDatas.Add(ProcessRocketMovement(null, gridState, pos2, Vector2Int.down));
                rocketBlastDatas.Add(ProcessRocketMovement(null, gridState, pos2, Vector2Int.up));
            }
        }
        IncrementRockets(rocketCount);
        //GridEvents.TriggerRocketBlastStarted(rocketBlastDatas);
        GridEvents.TriggerRocketBlastCombo(new RocketBlastCombo(center,itemsToAnimate,rocketBlastDatas));
    }






    public static void HandleSingleRocketHelper(GridState gridState, Vector2Int pos)
    {
        Rocket rocket = gridState.Get(pos) as Rocket;
        if (rocket == null) return;
        Vector2Int dir1, dir2;
        if (rocket.Direction == Rocket.RocketDirection.Horizontal)
        {
            dir1 = Vector2Int.left;
            dir2 = Vector2Int.right;

        }
        else
        {
            dir1 = Vector2Int.down;
            dir2 = Vector2Int.up;
        }
        IncrementRockets(2);
        GridItem originalRocket = gridState.Get(pos);
        gridState.Set(pos, null);
        List<RocketBlastData> rocketBlastDatas = new List<RocketBlastData>();
        rocketBlastDatas.Add(ProcessRocketMovement(originalRocket, gridState, pos, dir1));
        rocketBlastDatas.Add(ProcessRocketMovement(null, gridState, pos, dir2));
        GridEvents.TriggerRocketBlastStarted(rocketBlastDatas);
    }

    private static RocketBlastData ProcessRocketMovement(GridItem originalRocket, GridState gridState, Vector2Int startPos, Vector2Int direction)
    {

        Vector2Int nextPos = startPos + direction;

        Queue<AnimationType> animationQueue = new Queue<AnimationType>();
        Queue<GridItem> itemQueue = new Queue<GridItem>();


        while (gridState.IsValid(nextPos))
        {

            GridItem target = gridState.Get(nextPos);
            AnimationType animType = AnimationType.Null;

            if (target != null)
            {
                if (target.IsObstacle())
                {
                    Obstacle obs = target as Obstacle;
                    bool destroyed = obs.DealDamage(1, DamageSource.Rocket,gridState);

                    animType = destroyed ? AnimationType.Destruction : AnimationType.Damage;
                    if (destroyed)
                        gridState.Set(nextPos, null);
                }
                else if (target.IsCube())
                {
                    animType = AnimationType.Destruction;
                    gridState.Set(nextPos, null);
                }
                else if ((target as SpecialItem).IsRocket())
                {
                    animType = AnimationType.AnotherRocket;
                }
            }

            animationQueue.Enqueue(animType);
            itemQueue.Enqueue(target);

            nextPos += direction;
        }

        return new RocketBlastData(originalRocket, startPos, direction, animationQueue, itemQueue);
    }



    public static void IncrementRockets(int d)
    {
        Interlocked.Add(ref CurActiveRockets, d);
    }


    public static void DecrementRockets()
    {
        Interlocked.Decrement(ref CurActiveRockets);
        if (GetCurActiveRockets() == 0)
        {
            GridEvents.TriggerRocketLineClear();
        }
    }

    public static int GetCurActiveRockets()
    {
        return CurActiveRockets;
    }

}
public enum AnimationType
{
    Damage,
    Destruction,
    Null,
    AnotherRocket
}
