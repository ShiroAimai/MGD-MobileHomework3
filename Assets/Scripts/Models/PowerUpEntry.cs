using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class PowerUpEntry
    {
        public Sprite sprite;
        public PowerUp.Type powerUpType;
        public readonly Tile.TileType type = Tile.TileType.PowerUp;
    }
}