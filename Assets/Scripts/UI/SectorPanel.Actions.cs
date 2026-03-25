using UnityEngine;
using Settlers.Simulation;

namespace Settlers.UI
{
    public partial class SectorPanel
    {
        /// <summary>Attach a work yard to the first building with an open slot.</summary>
        private void TryAttachWorkYardInSector(int sectorId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State?.Construction == null) return;

            var buildings = gc.State.Construction.GetBuildingsInSector(sectorId);
            if (buildings == null) return;

            foreach (var building in buildings)
            {
                if (!building.CanAttachWorkYard) continue;

                // Find first compatible recipe for this building type
                var recipes = RecipeDatabase.GetForBuilding(building.Type);
                if (recipes == null || recipes.Count == 0) continue;

                var sector = gc.Graph.GetSector(sectorId);
                foreach (var recipe in recipes)
                {
                    // Check resource node requirement
                    if (recipe.RequiredNode != ResourceNodeType.None &&
                        !sector.HasResource(recipe.RequiredNode))
                        continue;

                    float offsetX = building.LocalX + (building.WorkYards.Count + 1) * 1.5f;
                    var wy = gc.AttachWorkYard(building.Id, recipe.WorkYardId,
                        offsetX, building.LocalZ);
                    if (wy != null)
                    {
                        ShowFeedback($"Attached {recipe.WorkYardId}", Color.green);
                        Refresh();
                        return;
                    }
                }
            }
        }

        /// <summary>Try to upgrade the first upgradeable building in a sector.</summary>
        private void TryUpgradeInSector(int sectorId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null) return;

            var buildings = gc.State?.Construction?.GetBuildingsInSector(sectorId);
            if (buildings == null) return;

            foreach (var building in buildings)
            {
                if (gc.TryUpgradeBuilding(building.Id))
                {
                    ShowFeedback($"Upgrading {building.Type}", Color.green);
                    break;
                }
            }
            Refresh();
        }

        /// <summary>Send the first idle general to target sector.</summary>
        private void TrySendArmyToSector(int targetSectorId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State?.Army == null) return;

            var generals = gc.State.Army.GetGenerals(0);
            foreach (var gen in generals)
            {
                if (gen.IsMoving) continue;
                if (gen.SectorId == targetSectorId) continue;
                if (gen.TotalSoldiers == 0) continue;

                bool sent = gc.State.Army.MoveArmy(gen, targetSectorId);
                if (sent)
                {
                    ShowFeedback($"General #{gen.Id} marching", Color.green);
                    return;
                }
            }
            ShowFeedback("No available general", Color.yellow);
        }

        /// <summary>Start proselytism on a neutral sector.</summary>
        private void TryProselytism(int sectorId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State == null) return;

            var sector = gc.Graph.GetSector(sectorId);
            if (!sector.IsNeutral)
            {
                ShowFeedback("Only works on neutral sectors", new Color(1f, 0.4f, 0.4f));
                return;
            }

            int clericCount = sector.IsFortified ? 12 : 6;
            bool started = gc.State.Conquest.StartProselytism(0, sectorId, clericCount);
            if (started)
                ShowFeedback($"Proselytism started ({clericCount} clerics)", Color.green);
            else
                ShowFeedback("Cannot start proselytism", new Color(1f, 0.4f, 0.4f));
        }

        /// <summary>Cycle food setting on all buildings in a sector.</summary>
        private void CycleFoodInSector(int sectorId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State?.Construction == null) return;

            var buildings = gc.State.Construction.GetBuildingsInSector(sectorId);
            if (buildings == null) return;

            foreach (var building in buildings)
            {
                if (!building.IsOperational) continue;
                var next = building.FoodSetting switch
                {
                    FoodSetting.None => FoodSetting.Plain,
                    FoodSetting.Plain => FoodSetting.Fancy,
                    FoodSetting.Fancy => FoodSetting.None,
                    _ => FoodSetting.None
                };
                building.SetFoodSetting(next);
            }
            Refresh();
        }
    }
}
