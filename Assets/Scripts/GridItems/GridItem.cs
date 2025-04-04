using System.Collections;
using UnityEngine;

public abstract class GridItem
{
    public GameObject gameObject{get;private set;}
    public Vector2Int GridPosition { get; private set; }
    public ItemType ItemType { get; set; }  

    public GridItem(GameObject gameObject, ItemType gridItemType, int x, int y)
    {
        this.gameObject = gameObject;
        ItemType = gridItemType;
        SetPosition(x, y);
    }
    public virtual bool IsCube() => false;
    public virtual bool IsObstacle() => false;
    public virtual bool IsSpecialItem() => false;


    public virtual bool IsFallable() => false;



    public virtual void Blast()
    {
    }




    public void SetPosition(int x, int y)
    {
        GridPosition = new Vector2Int(x, y);
    }
}
