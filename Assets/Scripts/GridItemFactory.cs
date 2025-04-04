using UnityEngine;
using System.IO;
using System.Security.Cryptography.X509Certificates;

public class GridItemFactory : MonoBehaviour
{
    public static GridItemFactory Instance { get; private set; }
    public RectTransform imageTransform;

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
    private System.Random random = new System.Random();

    // Prefabs for different grid items
    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject yellowCubePrefab;
    public GameObject boxObstaclePrefab;
    public GameObject stoneObstaclePrefab;
    public GameObject vaseObstaclePrefab;
    public GameObject horizontalRocketPrefab;
    public GameObject verticalRocketPrefab;

    public Transform gridParentTransform;

    public GridItem[,] CreateGrid(LevelData levelData)
    {
        int gridWidth = levelData.grid_width;
        int gridHeight = levelData.grid_height;
        GridItem[,] gridComponents = new GridItem[gridWidth, gridHeight];
        GridPositionCalculator.Instance.Configure(levelData.grid_width, levelData.grid_height);
        imageTransform.sizeDelta = GridPositionCalculator.Instance.GetGridFrameSize();
        ItemType[,] gridMatrix = levelData.GetGridMatrix();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                ItemType itemType = gridMatrix[x, y];
                gridComponents[x, y] = CreateGridItemGameObject(itemType, x, y);
            }
        }

        return gridComponents;
    }


    private GameObject GetPrefab(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Red => redCubePrefab,
            ItemType.Green => greenCubePrefab,
            ItemType.Blue => blueCubePrefab,
            ItemType.Yellow => yellowCubePrefab,
            ItemType.Box => boxObstaclePrefab,
            ItemType.Stone => stoneObstaclePrefab,
            ItemType.Vase => vaseObstaclePrefab,
            ItemType.HorizontalRocket => horizontalRocketPrefab,
            ItemType.VerticalRocket => verticalRocketPrefab,
            _ => null
        };
    }

    private GridItem CreateLogicItem(GameObject gameObject, ItemType itemType, int x, int y)
    {
        return itemType switch
        {
            ItemType.Red => new Cube(gameObject, ItemType.Red, x, y),
            ItemType.Green => new Cube(gameObject, ItemType.Green, x, y),
            ItemType.Blue => new Cube(gameObject, ItemType.Blue, x, y),
            ItemType.Yellow => new Cube(gameObject, ItemType.Yellow, x, y),
            ItemType.Box => new BoxObstacle(gameObject, x, y),
            ItemType.Stone => new StoneObstacle(gameObject, x, y),
            ItemType.Vase => new VaseObstacle(gameObject, x, y),
            ItemType.HorizontalRocket => new Rocket(gameObject, Rocket.RocketDirection.Horizontal, x, y),
            ItemType.VerticalRocket => new Rocket(gameObject, Rocket.RocketDirection.Vertical, x, y),
            _ => null
        };
    }

    public GridItem CreateGridItemGameObject(ItemType itemType, int x, int y)
    {
         if(itemType == ItemType.Random)
            itemType =(ItemType)random.Next(0, 4);
        GameObject prefab = GetPrefab(itemType);
        if(prefab == null)
            Debug.Log("prefab null aga");
        GameObject instance = Instantiate(prefab, GridPositionCalculator.Instance.GetWorldPosition(x, y), Quaternion.identity, gridParentTransform);
        GridItem logicItem = CreateLogicItem(instance, itemType, x, y);
        instance.GetComponent<GridItemComponent>().Initialize(logicItem);
        return logicItem;
    }
    public GridItem CreateRandomRocket(int x, int y)
    {
        bool hor =(Random.Range(0, 2) == 0) ? true : false;
        return hor?CreateGridItemGameObject(ItemType.HorizontalRocket,x,y):CreateGridItemGameObject(ItemType.VerticalRocket,x,y);
    }
}

