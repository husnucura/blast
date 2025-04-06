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

    public GameObject Smoke;
    public GameObject Star;


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
        if (itemType == ItemType.Random)
            itemType = (ItemType)random.Next(0, 4);
        GameObject prefab = GetPrefab(itemType);
        if (prefab == null)
            Debug.Log("prefab null aga");
        GameObject instance = Instantiate(prefab, GridPositionCalculator.Instance.GetWorldPosition(x, y), Quaternion.identity, gridParentTransform);
        GridItem logicItem = CreateLogicItem(instance, itemType, x, y);
        instance.GetComponent<GridItemComponent>().Initialize(logicItem);
        return logicItem;
    }
    public GridItem CreateRandomRocket(int x, int y)
    {
        bool hor = (Random.Range(0, 2) == 0) ? true : false;
        return hor ? CreateGridItemGameObject(ItemType.HorizontalRocket, x, y) : CreateGridItemGameObject(ItemType.VerticalRocket, x, y);
    }
    public GameObject CreateSplitRocket(Vector2Int pos, Vector2Int direction)
    {
        ItemType itemType = direction == Vector2Int.left || direction == Vector2Int.right
                ? ItemType.HorizontalRocket
                : ItemType.VerticalRocket;
        GameObject gameObject = CreateGridItemGameObject(itemType, pos.x, pos.y).gameObject;
        if (direction == Vector2Int.left || direction == Vector2Int.down)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<GridItemComponent>().Sprites[1];
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<GridItemComponent>().Sprites[2];
        }
        return gameObject;



    }
    public GameObject CreateSmokeOrStar(Vector3 position, float smokeProb)
    {
        // Randomly decide whether to create smoke or star based on the percentage probability
        if (Random.Range(0f, 1f) <= smokeProb)
        {
            return CreateSmokeObject(position);
        }
        else
        {
            return CreateStar(position);
        }
    }

    public GameObject CreateSmokeObject(Vector3 position)
    {
        GameObject smokeEffect = Instantiate(Smoke);

        float randomXOffset = Random.Range(-0.2f, 0.2f) * GridPositionCalculator.squareWidth;
        float randomYOffset = Random.Range(-0.2f, 0.2f);

        smokeEffect.transform.position = new Vector3(position.x + randomXOffset, position.y + randomYOffset, position.z);

        smokeEffect.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        return smokeEffect;
    }
    public GameObject CreateStar(Vector3 position)
    {
        GameObject smokeEffect = Instantiate(Star);

        float randomXOffset = Random.Range(-0.4f, 0.4f) * GridPositionCalculator.squareWidth;
        float randomYOffset = Random.Range(-0.4f, 0.4f) * GridPositionCalculator.squareWidth;

        smokeEffect.transform.position = new Vector3(position.x + randomXOffset, position.y + randomYOffset, position.z);

        smokeEffect.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        return smokeEffect;
    }


}

