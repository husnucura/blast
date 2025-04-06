using System;
using System.Collections.Generic;
using UnityEngine;

public class GridState
{
    private GridItem[,] grid;
    public int vaseTarget = 0;
    public int boxTarget = 0;
    public int stoneTarget = 0;

    public GridState(int level)
    {
        grid = GridItemFactory.Instance.CreateGrid(LevelManager.LoadLevel(level));

        // Initialize counts for vase, box, and stone targets
        InitializeTargetCounts();
    }

    public  bool IsLevelFinished()
    {
        return vaseTarget == 0 && boxTarget == 0 && stoneTarget == 0;
    }
    private void InitializeTargetCounts()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                GridItem item = grid[x, y];

                if (item.IsObstacle())
                {

                    if (item is VaseObstacle)
                    {
                        vaseTarget++;
                    }
                    else if (item is BoxObstacle)
                    {
                        boxTarget++;
                    }
                    else if (item is StoneObstacle)
                    {
                        stoneTarget++;
                    }
                }
            }
        }
    }


    public GridItem Get(Vector2Int pos)
    {
        return IsValid(pos) ? grid[pos.x, pos.y] : null;
    }
    public GridItem Get(int x, int y)
    {
        return Get(new Vector2Int(x, y));
    }

    public void Set(Vector2Int pos, GridItem item)
    {
        if (IsValid(pos))
        {
            grid[pos.x, pos.y] = item;
            if (item != null)
                item.SetPosition(pos.x, pos.y);
        }

    }
    public void Set(int x, int y, GridItem item)
    {
        Set(new Vector2Int(x, y), item);

    }

    public bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
               pos.y >= 0 && pos.y < grid.GetLength(1);
    }

    public IEnumerable<GridItem> AllItems()
    {
        foreach (var item in grid)
        {
            yield return item;
        }
    }

    public int Width => grid.GetLength(0);
    public int Height => grid.GetLength(1);
    public List<GridItem> PerformBFS(Vector2Int startPosition)
    {
        List<GridItem> cubes = new List<GridItem>();
        Queue<Vector2Int> positionsToCheck = new Queue<Vector2Int>();
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        positionsToCheck.Enqueue(startPosition);
        visitedPositions.Add(startPosition);

        CubeColor targetItemType = (Get(startPosition) as Cube).CubeColor;

        while (positionsToCheck.Count > 0)
        {
            Vector2Int currentPos = positionsToCheck.Dequeue();
            GridItem currentItemComponent = Get(currentPos);

            Cube currentCube = currentItemComponent as Cube;


            cubes.Add(currentCube);


            foreach (Vector2Int neighborPos in GetAdjacentPositions(currentPos))
            {
                if (!(Get(neighborPos) != null && Get(neighborPos).IsCube()))
                    continue;
                Cube nextCube = Get(neighborPos) as Cube;
                if (nextCube != null && !visitedPositions.Contains(neighborPos) && nextCube.CubeColor == targetItemType)
                {
                    visitedPositions.Add(neighborPos);
                    positionsToCheck.Enqueue(neighborPos);
                }
            }
        }
        return cubes;
    }
    public List<Vector2Int> GetAdjacentPositions(Vector2Int pos)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };

        List<Vector2Int> adjacentPositions = new List<Vector2Int>();

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = pos + direction;
            if (IsValid(neighborPos))
                adjacentPositions.Add(neighborPos);
        }

        return adjacentPositions;
    }

    public List<List<GridItem>> FindAllCubeGroups()
    {
        List<List<GridItem>> allCubeGroups = new List<List<GridItem>>();
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);

                // Skip if the position has already been visited or if it's not a cube
                if (visitedPositions.Contains(currentPos) || Get(currentPos) == null || !(Get(currentPos).IsCube()))
                    continue;

                // Perform BFS from the current position and get a group
                List<GridItem> group = PerformBFS(currentPos);
                if (group.Count > 0)
                {
                    // Add the group to the list of all groups
                    allCubeGroups.Add(group);

                    // Mark all positions in the group as visited
                    foreach (GridItem cube in group)
                    {
                        visitedPositions.Add(cube.GridPosition);
                    }
                }
            }
        }

        return allCubeGroups;
    }


    public List<Vector2Int> GetArea(Vector2Int pos)
    {
        Vector2Int[] directions = new Vector2Int[]
      {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1,1),
            new Vector2Int(-1, -1)
      };

        List<Vector2Int> adjacentPositions = new List<Vector2Int>();

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPos = pos + direction;
            if (IsValid(neighborPos))
                adjacentPositions.Add(neighborPos);
        }

        return adjacentPositions;
    }
}

