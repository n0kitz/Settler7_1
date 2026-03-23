using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// AI economy helper: building placement, work yard attachment, food management,
    /// building upgrades, and resource-aware decisions.
    /// Used by AIController. Pure C# — no UnityEngine references.
    /// </summary>
    public static class AIEconomy
    {
        public static void BuildEconomy(GameState state, int playerId)
        {
            var ownedSectors = state.Graph.GetSectorsOwnedBy(playerId);
            foreach (int sectorId in ownedSectors)
            {
                int buildCount = state.Construction.GetBuildingCountInSector(sectorId);
                var sector = state.Graph.GetSector(sectorId);
                if (buildCount >= sector.BuildSlots) continue;

                var type = ChooseBuildingType(state, playerId, sector);
                TryPlaceBuilding(state, playerId, type, sectorId);
            }
        }

        public static void AttachWorkYards(GameState state, int playerId)
        {
            foreach (var building in state.Construction.GetBuildingsByPlayer(playerId))
            {
                if (!building.IsOperational || !building.CanAttachWorkYard) continue;

                string wyId = ChooseWorkYard(state, playerId, building);
                if (wyId == null) continue;

                var recipe = RecipeDatabase.Get(wyId);
                if (recipe == null) continue;

                if (recipe.RequiredNode != ResourceNodeType.None)
                {
                    var sector = state.Graph.GetSector(building.SectorId);
                    if (!sector.HasResource(recipe.RequiredNode)) continue;
                }

                var wy = new WorkYard(wyId, building.Id, building.SectorId,
                    playerId, recipe.RequiredNode, 0f, 0f);
                if (building.AttachWorkYard(wy))
                    state.Production.RegisterWorkYard(wy);
            }
        }

        /// <summary>
        /// Manage food settings: enable plain food when sufficient supply,
        /// upgrade to fancy food when sausages are available.
        /// Disable food on buildings where supply ran out.
        /// </summary>
        public static void ManageFood(GameState state, int playerId)
        {
            int bread = GetResource(state, playerId, ResourceType.Bread);
            int fish = GetResource(state, playerId, ResourceType.Fish);
            int sausages = GetResource(state, playerId, ResourceType.Sausages);
            int plainFood = bread + fish;
            int fancyFood = sausages;

            foreach (var b in state.Construction.GetBuildingsByPlayer(playerId))
            {
                if (!b.IsOperational || b.WorkYards.Count == 0) continue;

                if (fancyFood >= 5 && b.FoodSetting != FoodSetting.Fancy)
                {
                    b.SetFoodSetting(FoodSetting.Fancy);
                }
                else if (plainFood >= 3 && b.FoodSetting == FoodSetting.None)
                {
                    b.SetFoodSetting(FoodSetting.Plain);
                }
                else if (plainFood < 1 && fancyFood < 1 &&
                         b.Type != BaseBuildingType.NobleResidence &&
                         b.FoodSetting != FoodSetting.None)
                {
                    // Disable food if supply is empty (prevents halt)
                    // But don't disable on NobleRes — it needs food to function at all
                }
            }
        }

        /// <summary>
        /// Try to upgrade buildings (Residence/NobleResidence) when affordable.
        /// </summary>
        public static void TryUpgradeBuildings(GameState state, int playerId)
        {
            foreach (var b in state.Construction.GetBuildingsByPlayer(playerId))
            {
                if (!b.IsOperational || !b.CanUpgrade) continue;

                // Only upgrade if we have surplus resources
                int planks = GetResource(state, playerId, ResourceType.Planks);
                int stone = GetResource(state, playerId, ResourceType.Stone);
                if (planks < 15 || stone < 8) continue;

                state.Upgrades.TryStartUpgrade(b.Id);
                break; // One upgrade at a time
            }
        }

        /// <summary>
        /// Accept and complete quests when possible.
        /// </summary>
        public static void ManageQuests(GameState state, int playerId)
        {
            // Try to accept available quests
            foreach (var quest in state.Quests.AvailableQuests)
            {
                if (quest.SectorId >= 0)
                {
                    var sector = state.Graph.GetSector(quest.SectorId);
                    if (sector.OwnerId != playerId) continue;
                }
                state.Quests.AcceptQuest(playerId, quest.Id);
                break; // One at a time
            }

            // Try to complete active quests
            var active = state.Quests.GetActiveQuests(playerId);
            foreach (var quest in active)
                state.Quests.TryCompleteQuest(playerId, quest.Id);
        }

        private static BaseBuildingType ChooseBuildingType(GameState state, int playerId, Sector sector)
        {
            int planks = GetResource(state, playerId, ResourceType.Planks);
            int wood = GetResource(state, playerId, ResourceType.Wood);
            int tools = GetResource(state, playerId, ResourceType.Tools);
            int food = GetResource(state, playerId, ResourceType.Bread) +
                       GetResource(state, playerId, ResourceType.Fish);
            int ironBars = GetResource(state, playerId, ResourceType.IronBars);

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

        private static string ChooseWorkYard(GameState state, int playerId, Building building)
        {
            var existing = new HashSet<string>();
            foreach (var wy in building.WorkYards) existing.Add(wy.TypeId);

            // Smarter priority based on current needs
            string[] priority = GetWorkYardPriority(state, playerId, building.Type);

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

        private static string[] GetWorkYardPriority(GameState state, int playerId, BaseBuildingType type)
        {
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

        public static bool TryPlaceBuilding(GameState state, int playerId,
            BaseBuildingType type, int sectorId)
        {
            var res = state.PlayerResources.TryGetValue(playerId, out var r) ? r : null;
            if (res == null) return false;

            BuildingCosts.Get(type, out int plankCost, out int stoneCost);
            if (!res.CanAfford(plankCost, stoneCost)) return false;

            var sector = state.Graph.GetSector(sectorId);
            int buildCount = state.Construction.GetBuildingCountInSector(sectorId);
            if (buildCount >= sector.BuildSlots) return false;

            res.TrySpendBuildingCost(plankCost, stoneCost);
            return state.Construction.PlaceBuilding(type, sectorId, playerId,
                3, 0f, buildCount * 3f, buildCount, sector.BuildSlots) != null;
        }

        public static int GetResource(GameState state, int playerId, ResourceType type) =>
            state.PlayerResources.TryGetValue(playerId, out var r) ? r.Get(type) : 0;
    }
}
