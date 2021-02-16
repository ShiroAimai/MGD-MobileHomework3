using System;
using Managers;
using Models;
using UnityEngine;

namespace Controllers
{
    [Serializable]
    public class FreezePowerUp : PowerUp
    {
        [SerializeField] private float freezeTimeDuration = 5f;

        public override void Execute(MatchContext context)
        {
            if(GameManager.Instance != null)
                GameManager.Instance.RequestFreezeTimeFor(freezeTimeDuration);
        }
    }
}