[System.Serializable]
public class LevelData {
    // JSON-mapped fields (must match exactly)
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;  // 1D array of item codes

    // Cached grid data (lazy-loaded)
    private ItemType[,] _gridMatrix;
    private bool _isGridParsed;

    /// <summary>
    /// Converts 1D JSON array to 2D grid matrix with proper coordinate mapping.
    /// Bottom-left is (0,0), top-right is (width-1, height-1)
    /// </summary>
    public  ItemType[,] GetGridMatrix() {
        if (_isGridParsed) return _gridMatrix;

        _gridMatrix = new ItemType[grid_width, grid_height];
        
        for (int y = 0; y < grid_height; y++) {
            for (int x = 0; x < grid_width; x++) {
                // Convert 1D index to 2D coordinates (bottom-left origin)
                int index = y * grid_width + x;
                _gridMatrix[x, y] = ParseItemType(grid[index]);
            }
        }

        _isGridParsed = true;
        return _gridMatrix;
    }

    /// <summary>
    /// Maps JSON item codes to strongly-typed enum values.
    /// Throws explicit errors for invalid codes during level loading.
    /// </summary>
    private ItemType ParseItemType(string code) => code switch {
        // Color cubes
        "r"    => ItemType.Red,
        "g"    => ItemType.Green,
        "b"    => ItemType.Blue,
        "y"    => ItemType.Yellow,
        "rand" => ItemType.Random,
        
        // Obstacles
        "bo"   => ItemType.Box,
        "s"    => ItemType.Stone,
        "v"    => ItemType.Vase,
        
        // Rockets
        "hro"  => ItemType.HorizontalRocket,
        "vro"  => ItemType.VerticalRocket,
        
        // Error handling
        ""     => ItemType.None,
        _      => throw new System.ArgumentException(
            $"Invalid item code '{code}' in level {level_number}. " +
            "Valid codes: r,g,b,y,rand,bo,s,v,hro,vro")
    };

    /// <summary>
    /// Editor validation (called from custom editor tools)
    /// </summary>
    public void Validate() {
        if (grid.Length != grid_width * grid_height) {
            throw new System.Exception(
                $"Level {level_number}: Grid size mismatch. " +
                $"Expected {grid_width}x{grid_height}={grid_width*grid_height} items, " +
                $"got {grid.Length}");
        }
    }
}