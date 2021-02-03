using UnityEngine;

namespace Utils
{
    public static class AdjacentDirections
    {
        public readonly static Vector2 Up = Vector2.up;
        public readonly static Vector2 Down = Vector2.down;
        public readonly static Vector2 Left = Vector2.left;
        public readonly static Vector2 Right = Vector2.right;

        public static Vector2 GetOpposite(this Vector2 dir)
        {
            //opposite direction of a vector is obtained by
            //-(1/magnitude)*vector
            return (-(1 / dir.magnitude)) * dir;
        }
        internal static class Bundle
        {
            public static readonly Vector2[] Horizontal = new Vector2[] {Left, Right};
            public static readonly Vector2[] Vertical = new Vector2[] {Up, Down};
            public static readonly Vector2[] All = new Vector2[] {Up, Down, Left, Right};
        }
    }
}