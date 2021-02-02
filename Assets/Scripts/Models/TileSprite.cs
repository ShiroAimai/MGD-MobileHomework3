using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public struct TileSprite
    {
        public Tile.TileType type;
        public Sprite sprite;
    }
}