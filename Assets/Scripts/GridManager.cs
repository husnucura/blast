using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public bool ResetLevel;
    public static GridManager Instance { get; private set; }
    private int AnimationsArePlaying = 0;

    private GridState gridState;
    private int[,] fallDistance;

    private void OnEnable()
    {
        GridEvents.OnGridCellClicked += HandleGridCellClicked;
        GridEvents.OnRocketBlastChainStarted += (pos) => RocketHandler.HandleSingleRocket(gridState, pos);
        GridEvents.OnRocketLineClear += (() => { HandleFallingObjects(); HandleNewObjects(); });
        GridEvents.OnGridUpdateAnimationFinished += HandleGridUpdateAnimationFinished;

    }
    private void OnDisable()
    {
        GridEvents.OnGridCellClicked -= HandleGridCellClicked;
        GridEvents.OnRocketBlastChainStarted -= (pos) => RocketHandler.HandleSingleRocket(gridState, pos);
        GridEvents.OnRocketLineClear -= (() => { HandleFallingObjects(); HandleNewObjects(); });
        GridEvents.OnGridUpdateAnimationFinished -= HandleGridUpdateAnimationFinished;

    }

    public void SetAnimationsPlaying(bool isPlaying)
    {
        int value = isPlaying ? 1 : 0;
        Interlocked.Exchange(ref AnimationsArePlaying, value);
    }
    public bool AreAnimationsPlaying()
    {
        return Interlocked.CompareExchange(ref AnimationsArePlaying, 0, 0) == 1;
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
        if (ResetLevel) LevelManager.SaveCurrentLevelNumber(1);
        LoadLevel(LevelManager.LoadCurrentLevelNumber());
    }

    public void LoadLevel(int levelNumber)
    {
        gridState = new GridState(levelNumber);
        fallDistance = new int[gridState.Width, gridState.Height];
        ShowRocketHint();
    }

    private void HandleGridCellClicked(Vector2Int gridPos)
    {
        if (AreAnimationsPlaying())
        {
            Debug.Log("There are gridsate changing animations or game is finished" + RocketHandler.CurActiveRockets);
            return;
        }
        if (!gridState.IsValid(gridPos)||gridState.Get(gridPos) == null) return;
        GridItem item = gridState.Get(gridPos);
        if (item.IsCube())
        {
            handleCubeBlast(gridPos);
        }
        else if (item.IsSpecialItem() && (item as SpecialItem).IsRocket())
        {
            SetAnimationsPlaying(true);
            gridState.RemainingMove--;
            gridState.SetRemainingmoves(gridState.RemainingMove);
            RocketHandler.HandleSingleRocket(gridState, gridPos, true);
        }

    }
    private void HandleObstacleDamages(List<GridItem> explodedCubes)
    {
        HashSet<Vector2Int> affectedPositions = new HashSet<Vector2Int>();

        List<GridItem> items = new List<GridItem>();
        foreach (var cube in explodedCubes)
        {
            foreach (var pos in gridState.GetAdjacentPositions(cube.GridPosition))
            {
                if (gridState.Get(pos) == null) continue;

                if (gridState.Get(pos).IsObstacle() && !affectedPositions.Contains(pos))
                {
                    Obstacle obstacle = gridState.Get(pos) as Obstacle;
                    bool destroyed = obstacle.DealDamage(1, DamageSource.Blast, gridState);
                    Debug.Log(pos);
                    if (destroyed)
                    {
                        items.Add(obstacle);

                        gridState.Set(pos, null);
                    }
                    else
                    {
                        StartCoroutine(obstacle.DealDamageAnimation());
                    }
                    affectedPositions.Add(pos);
                }
            }
        }
        GridEvents.TriggerItemsDestroyed(items);
    }


    private void handleCubeBlast(Vector2Int gridPos)
    {
        List<GridItem> cubes = gridState.PerformBFS(gridPos);

        if (cubes.Count < 2)
            return;
        SetAnimationsPlaying(true);
        gridState.RemainingMove--;
        gridState.SetRemainingmoves(gridState.RemainingMove);
        foreach (Cube cube in cubes)
        {
            var cubepos = cube.GridPosition;
            gridState.Set(cubepos, null);
        }
        if (cubes.Count >= 4)
        {
            GridItem rocket = CreateRocket(gridPos);
            rocket.gameObject.SetActive(false);
            GridEvents.TriggerNewRocketCreated(new NewRocketData(new List<GridItem>(cubes), gridPos, rocket));
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
                    int delay = 0;
                    while (curNullCount > 0)
                    {
                        var newObject = GridItemFactory.Instance.CreateGridItemGameObject(ItemType.Random, x, y);
                        gridState.Set(x, y - curNullCount, newObject);
                        newItemDatas.Add(new NewItemData(newObject, curNullCount, new Vector2Int(x, y), delay++));
                        curNullCount--;
                    }


                }
            }
            int delay2 = 0;
            while (curNullCount > 0)
            {
                var newObject = GridItemFactory.Instance.CreateGridItemGameObject(ItemType.Random, x, gridState.Height);
                newObject.gameObject.SetActive(false);
                gridState.Set(x, gridState.Height - curNullCount, newObject);
                newItemDatas.Add(new NewItemData(newObject, curNullCount, new Vector2Int(x, gridState.Height), delay2++));

                curNullCount--;
            }
        }
        GridEvents.TriggerNewItemsCreated(newItemDatas);
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

    private void ShowRocketHint()
    {
        List<List<GridItem>> groups = gridState.FindAllCubeGroups();
        foreach (List<GridItem> gridItems in groups)
        {
            bool should = gridItems.Count >= 4;
            foreach (GridItem gridItem in gridItems)
            {
                (gridItem as Cube).ShowHint(should);
            }
        }

    }
    public void HandleGridUpdateAnimationFinished()
    {
        ShowRocketHint();
        gridState.SetScoreVisuals();
        if (!CheckObstaclesCleared())
        {
            if (gridState.RemainingMove > 0)

            {
                SetAnimationsPlaying(false);
            }
            else
            {
                Debug.Log("fail");
                UiAnimationController.Instance.PlayLossAnimation();

            }
        }
        else if (gridState.RemainingMove >= 0)
        {
            Debug.Log("win");
            UiAnimationController.Instance.PlayWinAnimation();
        }
        else
        {
            Debug.Log("fail");
            UiAnimationController.Instance.PlayLossAnimation();

        }
    }



    private bool CheckObstaclesCleared()
    {
        return gridState.IsLevelFinished();
    }

    private void ShowWinParticles()
    {
        Debug.Log("Level Won! Showing celebration particles.");
    }
}
