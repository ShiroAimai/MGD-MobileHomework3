using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public struct TileEntry
    {
        public Tile.TileType type;
        public Sprite sprite;
    }
}