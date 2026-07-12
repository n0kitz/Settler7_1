namespace Settlers.Simulation
{
    /// <summary>
    /// Types of resource deposits found in sectors.
    /// </summary>
    public enum ResourceNodeType
    {
        None,
        Coal,
        Iron,
        Gold,
        Stone,
        FertileLand,
        Forest,
        FishingGround,
        WaterSource
    }

    /// <summary>
    /// Methods by which a sector can be conquered.
    /// </summary>
    public enum ConquestMethod
    {
        None,
        Military,
        Proselytism,
        Bribery
    }

    /// <summary>
    /// The five base building types that can be placed in a sector.
    /// Each type supports different work yard attachments.
    /// </summary>
    public enum BaseBuildingType
    {
        Lodge,           // 3 Planks, 1 pop — Forester, Woodcutter, Sawmill, Fisher, Hunter
        Farm,            // 3 Planks, 1 pop — Grain Barn, Windmill, Piggery, Shepherd, Stable
        MountainShelter, // 2P+1S, 1 pop — Quarry, Coal/Iron/Gold Miner, Iron Smelter, Coking Plant
        Residence,       // 2P+1S, 4 pop — Bakery, Brewery, Paper Mill, Weaving Mill, Wheelwright, Toolmaker
        NobleResidence   // 3P+2S, 5 pop — Butcher, Blacksmith, Mint, Goldsmith, Bookbinder, Tailor
    }

    /// <summary>
    /// All resource types in the game (raw and processed).
    /// </summary>
    public enum ResourceType
    {
        // Raw resources
        Wood,
        Stone,
        Coal,
        IronOre,
        GoldOre,
        Grain,
        Fish,
        Animal,
        Water,
        Wool,

        // Processed resources
        Planks,
        IronBars,
        Flour,
        Bread,
        Sausages,
        Beer,
        Paper,
        Books,
        Cloth,
        Garments,
        Coins,
        Weapons,
        Tools,
        Wheels,
        Horses,
        Jewelry,

        // §14.9 completion (Sprint 7c): Fleisch/Pelz/Leder chains + the two
        // trade-only luxuries (Gewürz/Wein arrive via trade outposts, no recipe)
        Meat,
        Fur,
        Leather,
        Spice,
        Wine
    }

    /// <summary>
    /// Food setting for a building's work yards.
    /// </summary>
    public enum FoodSetting
    {
        None,   // No food — Lodge/Farm/MtShelter/Res = x1, Noble = IDLE
        Plain,  // Bread/Fish — Lodge/Farm/MtShelter/Res = x2, Noble = x1
        Fancy   // Sausages — Lodge/Farm/MtShelter/Res = x3, Noble = x2
    }

    /// <summary>
    /// Construction state of a building.
    /// </summary>
    public enum BuildingState
    {
        Planned,       // Ghost placed, waiting for constructor
        UnderConstruction, // Constructor assigned, progress ticking
        Complete,      // Built and operational
        Upgrading      // Being upgraded to next level
    }
}
