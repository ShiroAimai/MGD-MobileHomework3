using System;
using UnityEngine;

namespace Controllers
{
    [Serializable]
    public class BombPowerUp : PowerUp
    {
        public static string BombAnimation = "Bomb";
        private const float BombRange = 1.5f;

        public override void Execute(MatchContext context)
        {
            Collider2D[] hits = new Collider2D[9];
            Physics2D.OverlapBoxNonAlloc(gameObject.transform.position, Vector2.one * BombRange, 0f, hits);

            for (int i = 0; i < hits.Length; ++i)
            {
                var nearbyTile = hits[i]?.gameObject?.GetComponent<TileController>();
                if(nearbyTile == null) continue; //tile is already destroyed
                if(nearbyTile == context._matchedTile || context._matches.Contains(nearbyTile)) continue; //already got that tile
                
                context._matches.Add(nearbyTile);
                if (nearbyTile.IsPowerUpTile() && !context._powerUps.Contains(nearbyTile))
                    context._powerUps.Add(nearbyTile);
            }
            gameObject.GetComponent<TileController>().Play(BombAnimation);

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(gameObject.transform.position, Vector2.one * BombRange);
        }
    }
}