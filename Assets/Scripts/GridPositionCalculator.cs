using UnityEngine;
public class GridPositionCalculator:MonoBehaviour
{
    public const float squareWidth = 1.4f;
    private const float sizePadding = 0.4f;
    private float xOffset;
    private float yOffset;
    private int gridWidth;
    private int gridHeight;

    public static GridPositionCalculator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    public void Configure(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        xOffset = squareWidth/2f * (width - 1);
        yOffset = squareWidth/2f * (height - 1);
    }

    public Vector2 GetGridSize()
    {
        return new Vector2(
            gridWidth * squareWidth + sizePadding,
            gridHeight * squareWidth + sizePadding
        );
    }

    public Vector3 GetWorldPosition(float x, float y)
    {
        return new Vector3(
            squareWidth * x - xOffset,
            squareWidth * y - yOffset,
            -y
        );
    }
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x + xOffset) / squareWidth);
        int y = Mathf.RoundToInt((worldPosition.y + yOffset) / squareWidth);
        return new Vector2Int(x, y);
    }
    public Vector2 GetGridFrameSize(){
       return  new Vector2(gridWidth*squareWidth+0.4f, gridHeight*squareWidth+0.4f);
    }
}