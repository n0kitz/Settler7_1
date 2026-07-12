using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// AIEconomy — building type and work-yard selection. When <c>prioritizeClergyGoods</c>
    /// is set (Technology-path AI, §14.6) the AI stands up its Bread/Books/Garments chain
    /// instead of luxury goods. Core economy orchestration lives in AIEconomy.cs.
    /// </summary>
    public static partial class AIEconomy
    {
        private static BaseBuildingType ChooseBuildingType(GameState state, int playerId,
            Sector sector, bool prioritizeClergyGoods = false)
        {
            int planks = GetResource(state, playerId, ResourceType.Planks);
            int wood = GetResource(state, playerId, ResourceType.Wood);
            int tools = GetResource(state, playerId, ResourceType.Tools);
            int food = GetResource(state, playerId, ResourceType.Bread) +
                       GetResource(state, playerId, ResourceType.Fish);
            int ironBars = GetResource(state, playerId, ResourceType.IronBars);

            // Technology AI needs Books (→ Brothers) and Garments (→ Fathers) for §14.6
            // research. Ensure the Books/Garments chain has homes — a Residence
            // (paper_mill, weaving_mill) and a Noble Residence (bookbinder, tailor) —
            // before piling on more mines or duplicates. Wood + basic food come first.
            if (prioritizeClergyGoods)
            {
                if (planks < 10 && wood < 5)
                    return BaseBuildingType.Lodge;
                // A Farm on a WaterSource + FertileLand sector unlocks the well (Water) plus
                // grain → flour — the Bread chain every cleric rank needs. Place it where the
                // water actually is, not just anywhere food happens to be short.
                if (sector.HasResource(ResourceNodeType.WaterSource) &&
                    sector.HasResource(ResourceNodeType.FertileLand) &&
                    !PlayerHasBuildingInSector(state, playerId, sector.Id, BaseBuildingType.Farm))
                    return BaseBuildingType.Farm;
                if (!HasBuildingType(state, playerId, BaseBuildingType.Farm) &&
                    sector.HasResource(ResourceNodeType.FertileLand))
                    return BaseBuildingType.Farm;
                if (!HasBuildingType(state, playerId, BaseBuildingType.Residence))
                    return BaseBuildingType.Residence;
                if (!HasBuildingType(state, playerId, BaseBuildingType.NobleResidence))
                    return BaseBuildingType.NobleResidence;
                // Chain hosts exist — fall through to the standard priorities.
            }

            // Priority: wood/planks → food → mining → tools/processed → luxury
            if (planks < 10 && wood < 5)
                return BaseBuildingType.Lodge;
            if (food < 5 && sector.HasResource(ResourceNodeType.FertileLand))
                return BaseBuildingType.Farm;
            if (sector.HasResource(ResourceNodeType.Iron) ||
                sector.HasResource(ResourceNodeType.Coal) ||
                sector.HasResource(ResourceNodeType.Stone))
                return BaseBuildingType.MountainShelter;
            if (tools < 5 && planks >= 2 && ironBars >= 1)
                return BaseBuildingType.Residence;
            if (planks >= 3 && ironBars >= 2)
                return BaseBuildingType.NobleResidence;
            return BaseBuildingType.Lodge;
        }

        private static string ChooseWorkYard(GameState state, int playerId, Building building,
            bool prioritizeClergyGoods = false)
        {
            var existing = new HashSet<string>();
            foreach (var wy in building.WorkYards) existing.Add(wy.TypeId);

            // Smarter priority based on current needs
            string[] priority = GetWorkYardPriority(state, playerId, building.Type,
                prioritizeClergyGoods);

            foreach (string wyId in priority)
            {
                if (existing.Contains(wyId)) continue;
                var recipe = RecipeDatabase.Get(wyId);
                if (recipe == null) continue;
                if (recipe.RequiredNode != ResourceNodeType.None)
                {
                    var sector = state.Graph.GetSector(building.SectorId);
                    if (!sector.HasResource(recipe.RequiredNode)) continue;
                }
                return wyId;
            }
            return null;
        }

        private static string[] GetWorkYardPriority(GameState state, int playerId,
            BaseBuildingType type, bool prioritizeClergyGoods = false)
        {
            // Technology AI biases toward the goods its clerics consume (§14.6):
            // Books = Wood+Water → Paper → Books; Garments = Wool → Cloth → Garments.
            // Bakery (Bread) and mint (Coins) stay high — recruiting spends both.
            if (prioritizeClergyGoods)
            {
                switch (type)
                {
                    // grain_barn + windmill + well complete the Bread chain (Grain → Flour,
                    // Water → Bread) that EVERY cleric rank needs; shepherd (Wool → Garments)
                    // follows for Father clerics. A well only attaches on a WaterSource sector.
                    case BaseBuildingType.Farm:
                        return new[] { "grain_barn", "windmill", "well", "shepherd", "piggery", "stable" };
                    // bakery (Bread), toolmaker (Tools — every work yard needs one, so the
                    // whole chain stalls without them) and paper_mill (Paper → Books) are the
                    // three Tier-2 essentials and fill a Residence's three slots exactly.
                    case BaseBuildingType.Residence:
                        return new[] { "bakery", "toolmaker", "paper_mill", "weaving_mill", "brewery", "wheelwright" };
                    case BaseBuildingType.NobleResidence:
                        return new[] { "bookbinder", "tailor", "mint", "butcher", "blacksmith", "goldsmith" };
                }
            }

            switch (type)
            {
                case BaseBuildingType.Lodge:
                {
                    int wood = GetResource(state, playerId, ResourceType.Wood);
                    int planks = GetResource(state, playerId, ResourceType.Planks);
                    // Prioritize sawmill if we have wood but low planks
                    if (wood >= 5 && planks < 10)
                        return new[] { "sawmill", "forester", "woodcutter", "fisher", "hunter", "well" };
                    return new[] { "forester", "woodcutter", "sawmill", "fisher", "hunter", "well" };
                }
                case BaseBuildingType.Farm:
                {
                    int grain = GetResource(state, playerId, ResourceType.Grain);
                    if (grain >= 5)
                        return new[] { "windmill", "piggery", "grain_barn", "shepherd", "stable" };
                    return new[] { "grain_barn", "windmill", "piggery", "shepherd", "stable" };
                }
                case BaseBuildingType.MountainShelter:
                {
                    int ironOre = GetResource(state, playerId, ResourceType.IronOre);
                    int coal = GetResource(state, playerId, ResourceType.Coal);
                    // Prioritize smelter if we have raw materials
                    if (ironOre >= 3 && coal >= 3)
                        return new[] { "iron_smelter", "iron_miner", "coal_miner", "quarry", "coking_plant", "gold_miner" };
                    return new[] { "iron_miner", "coal_miner", "quarry", "iron_smelter", "coking_plant", "gold_miner" };
                }
                case BaseBuildingType.Residence:
                {
                    int tools = GetResource(state, playerId, ResourceType.Tools);
                    if (tools < 3)
                        return new[] { "toolmaker", "bakery", "brewery", "wheelwright", "weaving_mill", "paper_mill" };
                    return new[] { "bakery", "toolmaker", "brewery", "wheelwright", "weaving_mill", "paper_mill" };
                }
                case BaseBuildingType.NobleResidence:
                    return new[] { "butcher", "blacksmith", "mint", "bookbinder", "tailor", "goldsmith" };
                default:
                    return System.Array.Empty<string>();
            }
        }

        private static bool HasBuildingType(GameState state, int playerId, BaseBuildingType type)
        {
            foreach (var b in state.Construction.GetBuildingsByPlayer(playerId))
                if (b.Type == type) return true;
            return false;
        }

        private static bool PlayerHasBuildingInSector(GameState state, int playerId,
            int sectorId, BaseBuildingType type)
        {
            foreach (var b in state.Construction.GetBuildingsByPlayer(playerId))
                if (b.SectorId == sectorId && b.Type == type) return true;
            return false;
        }
    }
}
