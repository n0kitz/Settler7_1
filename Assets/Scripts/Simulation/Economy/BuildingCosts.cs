namespace Settlers.Simulation
{
    /// <summary>
    /// Single source of truth for building placement costs.
    /// Used by GameController, BuildingPlacer, AIEconomy.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public static class BuildingCosts
    {
        public static void Get(BaseBuildingType type, out int planks, out int stone)
        {
            switch (type)
            {
                case BaseBuildingType.Lodge:           planks = 3; stone = 0; break;
                case BaseBuildingType.Farm:            planks = 3; stone = 0; break;
                case BaseBuildingType.MountainShelter: planks = 2; stone = 1; break;
                case BaseBuildingType.Residence:       planks = 2; stone = 1; break;
                case BaseBuildingType.NobleResidence:  planks = 3; stone = 2; break;
                default:                               planks = 0; stone = 0; break;
            }
        }
    }
}
