using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// AI economy helper: building placement, work yard attachment, food management,
    /// building upgrades, and resource-aware decisions.
    /// Used by AIController. Pure C# — no UnityEngine references.
    /// Building/work-yard selection lives in AIEconomy.BuildingChoice.cs.
    /// </summary>
    public static partial class AIEconomy
    {
        /// <summary>
        /// How far the AI may let total work-yard capacity (3 slots per building) run
        /// ahead of its living space before it stops raising more utility buildings.
        /// A utility building nets negative settlers when fully yarded, so an unchecked
        /// build-out ends with a swarm of yards no population can ever staff.
        /// </summary>
        private const int BUILD_SLOT_SLACK = 12;

        /// <summary>Work-yard slots the AI equips per building (matches TryPlaceBuilding).</summary>
        private const int AI_YARDS_PER_BUILDING = 3;

        public static void BuildEconomy(GameState state, int playerId,
            bool prioritizeClergyGoods = false)
        {
            // Don't out-build the population: once committed work-yard slots outrun living
            // space (plus a starter buffer), only population homes are still worth raising.
            int livingSpace = state.Population.GetLivingSpace(playerId);
            int opBuildings = 0;
            foreach (var b in state.Construction.GetBuildingsByPlayer(playerId))
                if (b.IsOperational) opBuildings++;
            bool saturated = opBuildings * AI_YARDS_PER_BUILDING > livingSpace + BUILD_SLOT_SLACK;

            var ownedSectors = state.Graph.GetSectorsOwnedBy(playerId);
            foreach (int sectorId in ownedSectors)
            {
                int buildCount = state.Construction.GetBuildingCountInSector(sectorId);
                var sector = state.Graph.GetSector(sectorId);
                if (buildCount >= sector.BuildSlots) continue;

                var type = ChooseBuildingType(state, playerId, sector, prioritizeClergyGoods);
                bool isPopHome = type == BaseBuildingType.Residence ||
                                 type == BaseBuildingType.NobleResidence;
                if (saturated && !isPopHome) continue;

                TryPlaceBuilding(state, playerId, type, sectorId);
            }
        }

        public static void AttachWorkYards(GameState state, int playerId,
            bool prioritizeClergyGoods = false)
        {
            // Only attach as many work yards as the population can actually staff — a
            // swarm of idle yards just spreads the scarce settlers and tools too thin.
            int settlerBudget = state.Population.GetAvailableSettlers(playerId);
            if (settlerBudget <= 0) return;

            foreach (var building in state.Construction.GetBuildingsByPlayer(playerId))
            {
                if (settlerBudget <= 0) break;
                if (!building.IsOperational || !building.CanAttachWorkYard) continue;

                string wyId = ChooseWorkYard(state, playerId, building, prioritizeClergyGoods);
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
                {
                    state.Production.RegisterWorkYard(wy);
                    settlerBudget--;
                }
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
                    b.SetFoodSetting(FoodSetting.None);
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
                AI_YARDS_PER_BUILDING, 0f, buildCount * 3f, buildCount, sector.BuildSlots) != null;
        }

        public static int GetResource(GameState state, int playerId, ResourceType type) =>
            state.PlayerResources.TryGetValue(playerId, out var r) ? r.Get(type) : 0;
    }
}
