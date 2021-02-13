using UnityEngine;

namespace Models
{
    public abstract class PowerUp : MonoBehaviour
    {
        protected static class PowerUpAnimation
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

        public abstract void Execute(MatchContext context);
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
}