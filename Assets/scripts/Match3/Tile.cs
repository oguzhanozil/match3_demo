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
        }

        public TileType tileType;

        public bool IsMatch(Tile otherTile)
        {
            return this.tileType == otherTile.tileType;
        }

    }
}