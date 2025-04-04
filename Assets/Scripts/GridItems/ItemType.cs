public enum ItemType {
    // Basic color cubes (matchable items)
    Red,        // "r"
    Green,      // "g"
    Blue,       // "b"
    Yellow,     // "y"
    Random,     // "rand" (spawns as random color cube)
    
    // Obstacles (must be cleared to win)
    Box,        // "bo" (breaks when adjacent cubes are blasted)
    Stone,      // "s" (only breaks when hit by rockets)
    Vase,       // "v" (requires 2 hits, can fall)
    
    // Special items
    HorizontalRocket,   // "hro" (created from 4+ cube matches)
    VerticalRocket,     // "vro" (created from 4+ cube matches)
    
    // Helper types
    None,       // Empty cell
    Blocked,    // Unused in current design (for future expansion)
    Bomb        // Example for future power-ups
}