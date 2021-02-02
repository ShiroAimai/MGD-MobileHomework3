using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class Tile
    {
        public enum TileType
        {
            Type1,
            Type2,
            Type3,
            Type4,
            Type5,
            Type6,
            TypeSuperPower
        }
        
        public TileType type;
        public int idRow, idColumn;
    }
    
}