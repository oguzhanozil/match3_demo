using UnityEngine;

namespace Match3
{
    public class Tile : MonoBehaviour
    {
        public enum TileType
        {
            Type1,
            Type2,
            Type3,
            // Add more tile types as needed
        }

        public TileType tileType;

        // Method to check if this tile matches with another tile
        public bool IsMatch(Tile otherTile)
        {
            return this.tileType == otherTile.tileType;
        }

        // Additional methods for tile behavior can be added here
    }
}