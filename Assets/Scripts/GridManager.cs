using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int level;
    public static GridManager Instance { get; private set; }

    private GridState gridState;
    private int[,] fallDistance;

    private void OnEnable()
    {
        GridEvents.OnGridCellClicked += HandleGridCellClicked;
    }

    private void OnDisable()
    {
        GridEvents.OnGridCellClicked -= HandleGridCellClicked;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadLevel(level);
    }

    public void LoadLevel(int levelNumber)
    {
        gridState = new GridState(levelNumber);
        fallDistance = new int[gridState.Width, gridState.Height];
    }

    private void HandleGridCellClicked(Vector2Int gridPos)
    {
        Debug.Log("Azd1");
        if (gridState.Get(gridPos) == null) return;
        Debug.Log("Azd2");
        GridItem item = gridState.Get(gridPos);
        if (item.IsCube())
        {
            Debug.Log("Azd3");

            handleCubeBlast(gridPos);
        }
        else if (item.IsSpecialItem() && (item as SpecialItem).IsRocket())
        {
            handleRocket(gridPos);
        }

        //CheckLevelWin();
    }

    private void handleRocket(Vector2Int gridPos)
    {
        return;
    }

    private void HandleObstacleDamages(List<GridItem> explodedCubes)
    {
        HashSet<Vector2Int> affectedPositions = new HashSet<Vector2Int>();

        List<GridItem> items = new List<GridItem>();
        foreach (var cube in explodedCubes)
        {
            foreach (var pos in GetAdjacentPositions(cube.GridPosition))
            {
                if (gridState.Get(pos) == null) continue;

                if (gridState.Get(pos).IsObstacle() && !affectedPositions.Contains(pos))
                {
                    Obstacle obstacle = gridState.Get(pos) as Obstacle;
                    bool destroyed = obstacle.DealDamage(1, DamageSource.Blast);
                    Debug.Log(pos);
                    if (destroyed)
                    {
                        items.Add(obstacle);

                        gridState.Set(pos, null);
                    }
                    affectedPositions.Add(pos);
                }
            }
        }
        GridEvents.TriggerItemsDestroyed(items);
    }


    private void handleCubeBlast(Vector2Int gridPos)
    {
        List<GridItem> cubes = PerformBFS(gridPos);

        if (cubes.Count < 2)
            return;

        foreach (Cube cube in cubes)
        {
            var cubepos = cube.GridPosition;
            gridState.Set(cubepos, null);
        }
        Debug.Log("Blaaaaaaasssssstttt " + cubes.Count);
        if (cubes.Count >= 4)
        {
            GridItem rocket = CreateRocket(gridPos);
            rocket.gameObject.SetActive(false);
            GridEvents.TriggerNewRocketCreated(new NewRocketData(new List<GridItem>(cubes),gridPos,rocket));
        }
        else
        {
            GridEvents.TriggerItemsDestroyed(new List<GridItem>(cubes));
        }
        HandleObstacleDamages(cubes);


        HandleFallingObjects();
        HandleNewObjects();
    }

    private void HandleNewObjects()
    {

        List<NewItemData> newItemDatas = new List<NewItemData>();
        for (int x = 0; x < gridState.Width; x++)
        {
            int curNullCount = 0;
            for (int y = 0; y < gridState.Height; y++)
            {
                if (gridState.Get(x, y) == null)
                {
                    curNullCount++;


                }
                else
                {
                    while (curNullCount > 0)
                    {
                        var newObject = GridItemFactory.Instance.CreateGridItemGameObject(ItemType.Random, x, y);
                        gridState.Set(x, y - curNullCount, newObject);
                        newItemDatas.Add(new NewItemData(newObject, curNullCount, new Vector2Int(x, y)));
                        curNullCount--;
                    }


                }
            }
            while (curNullCount > 0)
            {
                var newObject = GridItemFactory.Instance.CreateGridItemGameObject(ItemType.Random, x, gridState.Height);
                newObject.gameObject.SetActive(false);
                gridState.Set(x, gridState.Height - curNullCount, newObject);
                newItemDatas.Add(new NewItemData(newObject, curNullCount, new Vector2Int(x, gridState.Height)));

                curNullCount--;
            }
        }
        GridEvents.TriggerNewItemsCreated(newItemDatas);
    }


    private List<GridItem> PerformBFS(Vector2Int startPosition)
    {
        List<GridItem> cubes = new List<GridItem>();
        Queue<Vector2Int> positionsToCheck = new Queue<Vector2Int>();
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        positionsToCheck.Enqueue(startPosition);
        visitedPositions.Add(startPosition);

        CubeColor targetItemType = (gridState.Get(startPosition) as Cube).CubeColor;

        while (positionsToCheck.Count > 0)
        {
            Vector2Int currentPos = positionsToCheck.Dequeue();
            GridItem currentItemComponent = gridState.Get(currentPos);

            Cube currentCube = currentItemComponent as Cube;


            cubes.Add(currentCube);


            foreach (Vector2Int neighborPos in GetAdjacentPositions(currentPos))
            {
                if (!(gridState.Get(neighborPos) != null && gridState.Get(neighborPos).IsCube()))
                    continue;
                Cube nextCube = gridState.Get(neighborPos) as Cube;
                if (nextCube != null && !visitedPositions.Contains(neighborPos) && nextCube.CubeColor == targetItemType)
                {
                    visitedPositions.Add(neighborPos);
                    positionsToCheck.Enqueue(neighborPos);
                }
            }
        }
        return cubes;
    }

    private List<Vector2Int> GetAdjacentPositions(Vector2Int pos)
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
            if (gridState.IsValid(neighborPos))
                adjacentPositions.Add(neighborPos);
        }

        return adjacentPositions;
    }



    private GridItem CreateRocket(Vector2Int position)
    {
        GridItem gridItem = GridItemFactory.Instance.CreateRandomRocket(position.x, position.y);
        gridState.Set(position, gridItem);
        return gridItem;

    }

    private void HandleFallingObjects()
    {
        List<FallData> fallDatas = new List<FallData>();
        for (int x = 0; x < gridState.Width; x++)
        {

            if (gridState.Get(x, 0) == null)
            {
                fallDistance[x, 0] = 1;
            }
            else
            {
                fallDistance[x, 0] = 0;
            }

        }
        for (int y = 1; y < gridState.Height; y++)
            for (int x = 0; x < gridState.Width; x++)
            {
                {
                    int fallDist = fallDistance[x, y - 1];

                    if (gridState.Get(x, y) == null || gridState.Get(x, y).ItemType == ItemType.None)
                    {
                        fallDist++;
                    }

                    if (gridState.Get(x, y) != null && !gridState.Get(x, y).IsFallable())
                    {
                        fallDistance[x, y] = 0;
                    }
                    else
                    {
                        fallDistance[x, y] = fallDist;
                    }
                }
            }
        for (int y = 1; y < gridState.Height; y++)
            for (int x = 0; x < gridState.Width; x++)
            {
                {

                    if (gridState.Get(x, y) != null && gridState.Get(x, y).IsFallable() && fallDistance[x, y] > 0)
                    {
                        fallDatas.Add(new FallData(gridState.Get(x, y), fallDistance[x, y]));
                        gridState.Set(x, y - fallDistance[x, y], gridState.Get(x, y));
                        gridState.Set(x, y, null);
                    }
                }
            }
        GridEvents.TriggerItemsFall(new List<FallData>(fallDatas));
    }

    private void AssertGridConsistency()
    {
        System.Text.StringBuilder gridOutput = new System.Text.StringBuilder();

        // Loop through all positions in the grid
        for (int y = gridState.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridState.Width; x++)
            {
                // If the grid at this position is not null
                if (gridState.Get(x, y) != null)
                {
                    // Check if the position of the grid item matches the current x, y indices
                    Vector2Int gridPosition = gridState.Get(x, y).GridPosition;
                    if (gridPosition.x != x || gridPosition.y != y)
                    {
                        gridOutput.AppendLine($"Mismatch detected at GridPosition ({x}, {y}). " +
                            $"Expected GridPosition: ({x}, {y}), but got: ({gridPosition.x}, {gridPosition.y})");
                    }
                    else
                    {
                        gridOutput.Append($"[{gridState.Get(x, y).ItemType}] ");  // You can change `ItemType` to whatever field represents the item
                    }
                }
                else
                {
                    gridOutput.Append("[NULL] ");
                }
            }
            gridOutput.AppendLine(); // Start a new line for the next row
        }

        // Log the grid with detailed information
        Debug.Log(gridOutput.ToString());
    }



    private void CheckLevelWin()
    {

    }

    private void ShowWinParticles()
    {
        Debug.Log("Level Won! Showing celebration particles.");
    }
}
