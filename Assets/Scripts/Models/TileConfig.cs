using System;
using System.Collections.Generic;
using Controllers;
using NUnit.Framework;
using UnityEngine;

namespace Models
{
    [CreateAssetMenu(fileName = "TileConfig", menuName = "ScriptableObject/CreateTileConfig", order = 1)]
    public class TileConfig : ScriptableObject
    {
        public GameObject prefab;
        public List<TileEntry> entries;
        public PowerUp.Type powerUp;
        [UnityEngine.Range(0, 10)] public int spawnProbability;
    }
}