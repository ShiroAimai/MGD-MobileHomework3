using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class BombPowerUp : PowerUp
    {
        [SerializeField] private int bombRange = 1;
        
        public override void Execute(MatchContext context)
        {
            Collider2D[] hits = new Collider2D[9];
            Physics2D.OverlapBoxNonAlloc(gameObject.transform.position, Vector2.one * bombRange, 0f, hits);

            for (int i = 0; i < hits.Length; ++i)
            {
                var nearbyTile = hits[i]?.gameObject?.GetComponent<TileController>();
                if(nearbyTile == null) continue;
                if(nearbyTile == context._matchedTile || context._matches.Contains(nearbyTile)) continue; //already got that tile
                
                context._matches.Add(nearbyTile);
                if (nearbyTile.IsPowerUpTile() && !context._powerUps.Contains(nearbyTile))
                    context._powerUps.Add(nearbyTile);
                gameObject.GetComponent<TileController>().Play(PowerUpAnimation.Bomb);
            }
        }
    }
}