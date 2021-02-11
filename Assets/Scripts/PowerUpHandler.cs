using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Models;
using UnityEngine;

[Serializable]
public class PowerUpHandler
{
    [SerializeField] private List<PowerUpEntry> _powerUpEntries;
    
    [SerializeField] private FreezePowerUp freeze = new FreezePowerUp();
    [SerializeField] private BombPowerUp bomb = new BombPowerUp();

    public void HandlePowerUps(TileController matchedTile, List<TileController> matches)
    {
        if (!matchedTile.IsPowerUpTile() && !matches.Any(tile => tile.IsPowerUpTile())) return;
        {
            List<TileController> powerUps = matches.Where(tile => tile.IsPowerUpTile()).ToList();
            //also check matchedTile
            if(matchedTile.IsPowerUpTile())
                powerUps.Add(matchedTile);
            
            for (int i = 0; i < powerUps.Count; ++i)
            {
                switch (powerUps[i].GetPowerUpTile())
                {
                    case PowerUp.Type.Bomb:
                        bomb.Explode(powerUps[i], powerUps, matches);
                        break;
                    case PowerUp.Type.Freeze:
                        freeze.FreezeTime();
                        break;
                }                
            }
        }
    }

    public PowerUpEntry GetEntryFor(PowerUp.Type type)
    {
        return _powerUpEntries.FirstOrDefault(entry => entry.powerUpType == type);

    }
}