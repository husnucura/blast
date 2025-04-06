using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public static class RocketHandler
{
    // Tracks active rockets in each row (horizontal) and column (vertical)
    public static ConcurrentDictionary<int, int> currentRocketsAtRowY = new ConcurrentDictionary<int, int>();
    public static ConcurrentDictionary<int, int> currentRocketsAtColumnX = new ConcurrentDictionary<int, int>();


    public static void HandleSingleRocket(GridState gridState, Vector2Int pos)
    {
        Rocket rocket = gridState.Get(pos) as Rocket;
        if (rocket == null) return;
        Vector2Int dir1, dir2;
        if (rocket.Direction == Rocket.RocketDirection.Horizontal)
        {
            dir1 = Vector2Int.left;
            dir2 = Vector2Int.right;
            IncrementRocketCount(currentRocketsAtRowY, pos.y, 2);

        }
        else
        {
            dir1 = Vector2Int.down;
            dir2 = Vector2Int.up;
            IncrementRocketCount(currentRocketsAtColumnX, pos.x, 2);
        }
        GridItem originalRocket = gridState.Get(pos);
        gridState.Set(pos, null);
        ProcessRocketMovement(originalRocket, gridState, pos, dir1);
        ProcessRocketMovement(originalRocket, gridState, pos, dir2);
    }

    private static void ProcessRocketMovement(GridItem originalRocket, GridState gridState, Vector2Int startPos, Vector2Int direction)
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
                    bool destroyed = obs.DealDamage(1, DamageSource.Rocket);

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

        GridEvents.TriggerRocketBlastStarted(new RocketBlastData(originalRocket, startPos, direction, animationQueue, itemQueue));
    }


    public static void IncrementRocketCount(ConcurrentDictionary<int, int> dict, int key, int d)
    {
        dict.AddOrUpdate(key, d, (k, oldValue) => oldValue + d);
    }

    public static void DecrementRocketCount(ConcurrentDictionary<int, int> dict, int key)
    {
        bool updated = false;
        int newValue = 0;

        dict.AddOrUpdate(key, 0, (k, oldValue) =>
        {
            newValue = oldValue - 1;
            updated = true;
            return newValue;
        });

        if (updated && newValue <= 0)
        {
            dict.TryRemove(key, out _);
            GridEvents.TriggerRocketLineClear();
        }
    }

}
public enum AnimationType
{
    Damage,
    Destruction,
    Null,
    AnotherRocket
}
