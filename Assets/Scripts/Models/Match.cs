using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    [Serializable]
    public struct Match
    {
        public static int MatchMinLength = 2;
            
        public Vector2 dir;
        public List<GameObject> matches;
    }
}