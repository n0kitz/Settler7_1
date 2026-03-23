namespace Settlers.Simulation
{
    /// <summary>
    /// Military unit types trainable at the Stronghold.
    /// Each requires a prestige unlock and specific resources.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public enum UnitType
    {
        Pikeman,       // Basic infantry — prestige: mil_pikeman
        Musketeer,     // Required to breach fortifications — prestige: mil_musketeer
        Cavalier,      // Fast, strong — prestige: mil_cavalier
        Cannon,        // Siege unit, breach fortifications — prestige: mil_cannon
        StandardBearer // Army morale bonus — prestige: mil_standard_bearer
    }

    /// <summary>
    /// Static unit stats database.
    /// </summary>
    public static class UnitStats
    {
        public static int GetAttack(UnitType type) => type switch
        {
            UnitType.Pikeman => 3,
            UnitType.Musketeer => 5,
            UnitType.Cavalier => 7,
            UnitType.Cannon => 10,
            UnitType.StandardBearer => 1,
            _ => 0
        };

        public static int GetDefense(UnitType type) => type switch
        {
            UnitType.Pikeman => 4,
            UnitType.Musketeer => 3,
            UnitType.Cavalier => 5,
            UnitType.Cannon => 2,
            UnitType.StandardBearer => 2,
            _ => 0
        };

        public static int GetHealth(UnitType type) => type switch
        {
            UnitType.Pikeman => 10,
            UnitType.Musketeer => 8,
            UnitType.Cavalier => 12,
            UnitType.Cannon => 6,
            UnitType.StandardBearer => 8,
            _ => 0
        };

        /// <summary>Can this unit breach fortified sectors?</summary>
        public static bool CanBreachFortification(UnitType type) =>
            type == UnitType.Musketeer || type == UnitType.Cannon;

        /// <summary>Get the prestige unlock ID required to train this unit.</summary>
        public static string GetRequiredUnlock(UnitType type) => type switch
        {
            UnitType.Pikeman => "mil_pikeman",
            UnitType.Musketeer => "mil_musketeer",
            UnitType.Cavalier => "mil_cavalier",
            UnitType.Cannon => "mil_cannon",
            UnitType.StandardBearer => "mil_standard_bearer",
            _ => null
        };

        /// <summary>Resource cost to train one unit.</summary>
        public static void GetTrainingCost(UnitType type,
            out ResourceType resource, out int amount)
        {
            switch (type)
            {
                case UnitType.Pikeman:
                    resource = ResourceType.Weapons; amount = 1; break;
                case UnitType.Musketeer:
                    resource = ResourceType.Weapons; amount = 2; break;
                case UnitType.Cavalier:
                    resource = ResourceType.Horses; amount = 1; break;
                case UnitType.Cannon:
                    resource = ResourceType.IronBars; amount = 3; break;
                case UnitType.StandardBearer:
                    resource = ResourceType.Cloth; amount = 1; break;
                default:
                    resource = ResourceType.Weapons; amount = 0; break;
            }
        }

        /// <summary>Training time in seconds.</summary>
        public static float GetTrainingTime(UnitType type) => type switch
        {
            UnitType.Pikeman => 8f,
            UnitType.Musketeer => 12f,
            UnitType.Cavalier => 15f,
            UnitType.Cannon => 20f,
            UnitType.StandardBearer => 10f,
            _ => 10f
        };
    }
}
