using System;
using Controllers;

namespace Models
{
    [Serializable]
    public class Tile
    {
        private Tile() {}
        
        public enum TileType
        {
            Type1,
            Type2,
            Type3,
            Type4,
            Type5,
            Type6,
            PowerUp
        }
        
        public TileType type;
        public PowerUp.Type powerUpType = PowerUp.Type.None;
        public BoardPoint point;

        public bool IsPowerUpTile => type == TileType.PowerUp && 
                                     powerUpType != PowerUp.Type.None;
        
        public static Tile Create(TileType _type, int _idRow, int _idColumn, PowerUp.Type _superPowerType = PowerUp.Type.None)
        {
            var tile = new Tile() {type = _type, point = BoardPoint.Create(_idRow, _idColumn)};
            if (_type == TileType.PowerUp && _superPowerType != PowerUp.Type.None)
            {
                tile.powerUpType = _superPowerType;
            }

            return tile;
        }
    }
    
}