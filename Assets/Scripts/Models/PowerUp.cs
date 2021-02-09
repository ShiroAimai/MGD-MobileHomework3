using System.Collections.Generic;
using Controllers;
using Managers;
using UnityEngine;

namespace Models
{
    public abstract class PowerUp
    {
        public static class PowerUpAnimation
        {
            public static string Bomb => "Bomb";
        }
        
        public enum Type
        {
            None,
            Bomb,
            Freeze
        }
        
        public static Type FromString(string powerUpType)
        {
            if (Type.Bomb.ToPowerUpString() == powerUpType)
                return Type.Bomb;
            if (Type.Freeze.ToPowerUpString() == powerUpType)
                return Type.Freeze;
            return Type.None;
        }
        
    }

    public static class PowerUpTypeExtension
    {
        public static string ToPowerUpString(this PowerUp.Type type)
        {
            switch (type)
            {
                case PowerUp.Type.Bomb: return "Bomb";
                case PowerUp.Type.Freeze: return "Freeze";
                default: return "";
            }
        }
    }
    public class BombPowerUp : PowerUp
    {
        public void Explode(TileController powerUp, List<TileController> powerUps, List<TileController> matches)
        {
            Collider2D[] hits = new Collider2D[9];
            Physics2D.OverlapBoxNonAlloc(powerUp.transform.position, Vector2.one, 0f, hits);

            for (int i = 0; i < hits.Length; ++i)
            {
                var nearbyTile = hits[i]?.gameObject?.GetComponent<TileController>();
                if(nearbyTile == null) continue;
                if(matches.Contains(nearbyTile)) continue; //already got that tile
                
                matches.Add(nearbyTile);
                if(nearbyTile.IsPowerUpTile())
                    powerUps.Add(nearbyTile);
                powerUp.Play(PowerUpAnimation.Bomb);
            }
        }
    }
    
    public class FreezePowerUp : PowerUp
    {
        private const float freezeTimeDuration = 5f;
        
        public void FreezeTime() 
        {
            if(GameManager.Instance != null)
                GameManager.Instance.RequestFreezeTimeFor(freezeTimeDuration);
        }
    }
    
}