using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    // Singleton instance
    public static PrefabManager Instance { get; private set; }

    // Expose these prefabs in the Unity Inspector
    public GameObject RedCubePrefab;
    public GameObject BlueCubePrefab;
    public GameObject GreenCubePrefab;
    public GameObject YellowCubePrefab;

    public GameObject boxObstaclePrefab; // The prefab for Box obstacles
    public GameObject stoneObstaclePrefab; // The prefab for Stone obstacles
    public GameObject vaseObstaclePrefab; // The prefab for Vase obstacles
    public GameObject horizontalRocketPrefab;
    public GameObject verticalRocketPrefab;


    // Make sure the singleton instance is set
    private void Awake()
    {
        // If an instance already exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // Optionally, make the instance persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    // Method to get the appropriate prefab based on the type
    public GameObject GetPrefabForGridItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Red:
                return RedCubePrefab;
            case ItemType.Blue:
                return BlueCubePrefab;
            case ItemType.Green:
                return GreenCubePrefab;
            case ItemType.Yellow:
                return YellowCubePrefab;
            case ItemType.Box:
                return boxObstaclePrefab;
            case ItemType.Stone:
                return stoneObstaclePrefab;
            case ItemType.Vase:
                return vaseObstaclePrefab;
            case ItemType.HorizontalRocket:
                return horizontalRocketPrefab;
            case ItemType.VerticalRocket:
                return verticalRocketPrefab;
            default:
                Debug.LogError($"No prefab assigned for ItemType: {itemType}");
                return null;
        }
    }


    // Method to instantiate the prefab at a given position
    public GameObject InstantiatePrefab(GameObject prefab, Vector2Int position)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is missing!");
            return null;
        }
        return Instantiate(prefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
    }
}
