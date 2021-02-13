using System;
using Controllers;

namespace Models
{
    [Serializable]
    public class TileState
    {
        private TileState() {}
        
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
        public BoardPoint point;

        public bool IsPowerUpTile => type == TileType.PowerUp;
        
        public static TileState Create(TileType _type, int _idRow, int _idColumn)
        {
            return new TileState() {type = _type, point = BoardPoint.Create(_idRow, _idColumn)};
        }
    }
    
}