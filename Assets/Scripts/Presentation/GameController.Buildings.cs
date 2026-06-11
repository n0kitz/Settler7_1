using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Partial class for building placement and work yard attachment.
    /// </summary>
    public partial class GameController
    {
        // --- Building Placement ---

        private void HandleBuildMenuSelection(BaseBuildingType type)
        {
            _buildingPlacer.BeginPlacement(type);
        }

        private void HandleBuildingPlaced(int sectorId, BaseBuildingType type, Vector3 worldPos)
        {
            var sector = Graph.GetSector(sectorId);
            int playerId = sector.OwnerId;
            var resources = GetPlayerResources(playerId);

            if (!BuildingPrestigeGate.IsUnlocked(State.Prestige, playerId, type))
            {
                var def = PrestigeDatabase.Get(BuildingPrestigeGate.RequiredUnlock(type));
                if (_buildMenu != null)
                    _buildMenu.ShowFeedback(string.Format(
                        L.Get("ui.build.locked_feedback"),
                        def?.DisplayName ?? type.ToString()));
                return;
            }

            BuildingCosts.Get(type, out int plankCost, out int stoneCost);
            if (resources != null && !resources.TrySpendBuildingCost(plankCost, stoneCost))
            {
                Debug.LogWarning($"Cannot afford {type}: need {plankCost}P+{stoneCost}S, " +
                    $"have {resources.Get(ResourceType.Planks)}P+{resources.Get(ResourceType.Stone)}S");
                if (_buildMenu != null)
                    _buildMenu.ShowFeedback(string.Format(
                        L.Get("ui.build.no_resources"), plankCost, stoneCost));
                return;
            }

            int currentCount = Construction.GetBuildingCountInSector(sectorId);
            var building = Construction.PlaceBuilding(
                type, sectorId, playerId,
                maxWorkYards: 3,
                localX: worldPos.x, localZ: worldPos.z,
                currentBuildCount: currentCount, maxSlots: sector.BuildSlots);

            if (building != null)
            {
                var view = BuildingView.CreatePrimitive(
                    _buildingsRoot, building.Id, worldPos, type, _buildingMaterial);
                _buildingViews[building.Id] = view;

                Debug.Log($"Placed {type} in {sector.Name} (ID:{building.Id}, " +
                    $"Slot {currentCount + 1}/{sector.BuildSlots}, " +
                    $"Planks:{resources?.Get(ResourceType.Planks)}, Stone:{resources?.Get(ResourceType.Stone)})");
            }
        }

        /// <summary>
        /// Attach a work yard to a completed building.
        /// </summary>
        public WorkYard AttachWorkYard(int buildingId, string workYardTypeId,
            float localX, float localZ)
        {
            var building = Construction.GetBuilding(buildingId);
            if (building == null || !building.CanAttachWorkYard)
                return null;

            var recipe = RecipeDatabase.Get(workYardTypeId);
            if (recipe == null) return null;
            if (recipe.ParentBuilding != building.Type) return null;

            if (recipe.RequiredNode != ResourceNodeType.None)
            {
                var sector = Graph.GetSector(building.SectorId);
                if (!sector.HasResource(recipe.RequiredNode)) return null;
            }

            var wy = new WorkYard(workYardTypeId, buildingId, building.SectorId,
                building.OwnerId, recipe.RequiredNode, localX, localZ);

            if (!building.AttachWorkYard(wy)) return null;

            State.Production.RegisterWorkYard(wy);
            Events.Publish(new WorkYardAttachedEvent(wy.Id, buildingId, workYardTypeId));
            return wy;
        }

        private void UpdateBuildingViews()
        {
            foreach (var building in Construction.AllBuildings)
            {
                if (_buildingViews.TryGetValue(building.Id, out var view))
                    view.UpdateState(building.State, building.ConstructionProgress);
            }
        }

        /// <summary>Remove a building's view when it is destroyed (e.g., on conquest).</summary>
        public void HandleBuildingDestroyed(BuildingDestroyedEvent evt)
        {
            if (_buildingViews.TryGetValue(evt.BuildingId, out var view))
            {
                if (view != null)
                    Destroy(view.gameObject);
                _buildingViews.Remove(evt.BuildingId);
            }
        }

        internal Simulation.VPThresholds BuildVPThresholds()
        {
            if (_gameConstants == null) return new Simulation.VPThresholds();
            var gc = _gameConstants;
            return new Simulation.VPThresholds
            {
                FieldMarshalArmy = gc.vpFieldMarshalMin,
                MetropolisWorkers = gc.vpMetropolisMin,
                EmperorSectors = gc.vpEmperorMin,
                BankerCoins = gc.vpBankerMin,
                SunKingPrestige = gc.vpSunKingMin,
                TradingCompanyOutposts = gc.vpTradingCompanyMin,
                FountainTechs = gc.vpFountainMin,
                PacifistSeconds = gc.vpPacifistMinSeconds,
                EconomistStaffPercent = gc.vpEconomistMinPercent,
                GeneralissimoKills = gc.vpGeneralissimoMin
            };
        }
    }
}
