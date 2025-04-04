using System.Collections.Generic;
using UnityEngine;

public class GridState
{
    private GridItem[,] grid;

    public GridState(int level)
    {
        grid = GridItemFactory.Instance.CreateGrid(LevelLoader.LoadLevel(level));
    }

    public GridItem Get(Vector2Int pos)
    {
        return IsValid(pos) ? grid[pos.x, pos.y] : null;
    }
    public GridItem Get(int x,int y)
    {
        return Get(new Vector2Int(x,y));
    }

    public void Set(Vector2Int pos, GridItem item)
    {
        if (IsValid(pos)){
            grid[pos.x, pos.y] = item;
            if(item != null)
                item.SetPosition(pos.x,pos.y);
        }

    }
    public void Set(int x,int y, GridItem item)
    {
        Set(new Vector2Int(x,y),item);
        
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
}
