using System;
using Managers;
using UnityEngine;

namespace Models
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