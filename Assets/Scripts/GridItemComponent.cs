using UnityEngine;

public class GridItemComponent : MonoBehaviour
{
    public GridItem GridItem { get;set; }
    public  Sprite[] Sprites;

    public void Initialize(GridItem gridItem)
    {
        GridItem = gridItem;
    }
}
