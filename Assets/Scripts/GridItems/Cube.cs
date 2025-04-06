using System;
using System.Collections;
using UnityEngine;

public enum CubeColor
{
    Red,
    Blue,
    Green,
    Yellow,
}
public class Cube : GridItem
{
    public CubeColor CubeColor { get; private set; }

    public Cube(GameObject gameObject, ItemType itemType, int x, int y) : base(gameObject, itemType, x, y)
    {
        switch (itemType)
    {
        case ItemType.Red:
            CubeColor = CubeColor.Red;
            break;
        case ItemType.Green:
            CubeColor = CubeColor.Green;
            break;
        case ItemType.Blue:
            CubeColor = CubeColor.Blue;
            break;
        case ItemType.Yellow:
            CubeColor = CubeColor.Yellow;
            break;
        default:
            Debug.Log("This should not be printed");
            CubeColor = CubeColor.Red; // Or some default color
            break;
    }
    }


    private CubeColor GetRandomColor()
    {
        CubeColor[] colorValues = new CubeColor[]
        {
            CubeColor.Red,
            CubeColor.Blue,
            CubeColor.Green,
            CubeColor.Yellow
        };

        return colorValues[UnityEngine.Random.Range(0, colorValues.Length)];
    }

    public override bool IsFallable() => true;
    public override bool IsCube() => true;


    public override void Blast()
    {
        Debug.Log($"Cube at {GridPosition} blasted!");
    }

    public void ShowHint(bool should)
    {

    if(should) gameObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<GridItemComponent>().Sprites[1];
    else{
        gameObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<GridItemComponent>().Sprites[0];
    }

    }
}
