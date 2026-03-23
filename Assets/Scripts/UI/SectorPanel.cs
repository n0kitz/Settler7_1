using System.Collections;
using UnityEngine;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Side panel showing info about the currently selected sector.
    /// Reads from SectorGraph via GameController — never modifies simulation.
    /// </summary>
    public class SectorPanel : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI _sectorName;
        [SerializeField] private TextMeshProUGUI _ownerText;
        [SerializeField] private TextMeshProUGUI _garrisonText;
        [SerializeField] private TextMeshProUGUI _resourcesText;
        [SerializeField] private TextMeshProUGUI _buildSlotsText;
        [SerializeField] private TextMeshProUGUI _fortifiedText;
        [SerializeField] private TextMeshProUGUI _buildingsText;

        [Header("Hotkeys & Feedback")]
        [SerializeField] private TextMeshProUGUI _hotkeyHints;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private int _currentSectorId = -1;
        private Coroutine _feedbackCoroutine;
        private float _refreshTimer;

        /// <summary>Whether the panel is currently showing a sector.</summary>
        public bool IsVisible => _currentSectorId >= 0;

        private void Start()
        {
            Hide();
        }

        private void Update()
        {
            if (_currentSectorId < 0) return;

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            // F key cycles food setting on all buildings in selected sector
            if (kb.fKey.wasPressedThisFrame)
                CycleFoodInSector(_currentSectorId);

            // U key upgrades the first upgradeable building in selected sector
            if (kb.uKey.wasPressedThisFrame)
                TryUpgradeInSector(_currentSectorId);

            // W key attaches a work yard to the first building that can accept one
            if (kb.wKey.wasPressedThisFrame)
                TryAttachWorkYardInSector(_currentSectorId);

            // G key sends first available general to selected sector
            if (kb.gKey.wasPressedThisFrame)
                TrySendArmyToSector(_currentSectorId);

            // C key starts proselytism on selected neutral sector
            if (kb.cKey.wasPressedThisFrame)
                TryProselytism(_currentSectorId);

            // Auto-refresh every 0.5s when visible
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = 0.5f;
                Refresh();
            }
        }

        /// <summary>Show the panel with info for a given sector.</summary>
        public void ShowSector(int sectorId)
        {
            var graph = Presentation.GameController.Instance?.Graph;
            if (graph == null || sectorId < 0 || sectorId >= graph.SectorCount)
                return;

            _currentSectorId = sectorId;
            var sector = graph.GetSector(sectorId);

            if (_sectorName != null) _sectorName.text = sector.Name;
            if (_ownerText != null) _ownerText.text = FormatOwner(sector);
            if (_garrisonText != null) _garrisonText.text = sector.IsNeutral
                ? $"Garrison: {sector.GarrisonStrength}"
                : "";
            if (_resourcesText != null) _resourcesText.text = FormatResources(sector);
            int buildingCount = Presentation.GameController.Instance?.GetBuildingCountInSector(sectorId) ?? 0;
            if (_buildSlotsText != null) _buildSlotsText.text = $"Buildings: {buildingCount}/{sector.BuildSlots}";
            if (_fortifiedText != null) _fortifiedText.text = sector.IsFortified
                ? "Fortified"
                : "";
            if (_buildingsText != null) _buildingsText.text = FormatBuildings(sectorId);

            if (_panelRoot != null) _panelRoot.SetActive(true);
        }

        /// <summary>Hide the panel.</summary>
        public void Hide()
        {
            _currentSectorId = -1;
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        /// <summary>Refresh the currently shown sector (call after ownership changes).</summary>
        public void Refresh()
        {
            if (_currentSectorId >= 0)
                ShowSector(_currentSectorId);
        }

        private string FormatOwner(Sector sector)
        {
            if (sector.IsUnowned) return "Owner: None";
            if (sector.IsNeutral) return "Owner: Neutral";
            return $"Owner: Player {sector.OwnerId + 1}";
        }

        private string FormatResources(Sector sector)
        {
            if (sector.ResourceNodes.Count == 0)
                return "Resources: None";

            var sb = new System.Text.StringBuilder("Resources: ");
            for (int i = 0; i < sector.ResourceNodes.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(FormatResourceName(sector.ResourceNodes[i]));
            }
            return sb.ToString();
        }

        private string FormatBuildings(int sectorId)
        {
            var gc = Presentation.GameController.Instance;
            if (gc?.State?.Construction == null) return "";

            var buildings = gc.State.Construction.GetBuildingsInSector(sectorId);
            if (buildings == null || buildings.Count == 0) return "";

            var sb = new System.Text.StringBuilder();
            foreach (var building in buildings)
            {
                string state = building.IsOperational ? "" : " (Building)";
                string food = building.FoodSetting != FoodSetting.None
                    ? $" [{building.FoodSetting}]" : "";
                sb.AppendLine($"{building.Type}{state}{food}");

                foreach (var wy in building.WorkYards)
                {
                    var recipe = RecipeDatabase.Get(wy.TypeId);
                    string displayName = recipe?.DisplayName ?? wy.TypeId;

                    if (!wy.IsOperational)
                    {
                        sb.AppendLine($"  - {displayName} (idle)");
                        continue;
                    }

                    int pct = (int)(wy.CycleProgress * 100);
                    if (recipe != null && recipe.Outputs.Length > 0)
                    {
                        var output = recipe.Outputs[0];
                        float ratePerMin = 60f / recipe.CycleDuration * output.amount;
                        sb.AppendLine($"  - {displayName} {pct}% \u2192 {output.type} {ratePerMin:F1}/min");
                    }
                    else
                    {
                        sb.AppendLine($"  - {displayName} {pct}%");
                    }
                }
            }
            return sb.ToString();
        }

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

        private string FormatResourceName(ResourceNodeType nodeType)
        {
            return nodeType switch
            {
                ResourceNodeType.FertileLand => "Fertile Land",
                ResourceNodeType.FishingGround => "Fishing",
                ResourceNodeType.WaterSource => "Water",
                _ => nodeType.ToString()
            };
        }

        /// <summary>Show a feedback message that auto-hides after 2 seconds.</summary>
        private void ShowFeedback(string msg, Color color)
        {
            if (_feedbackText == null) return;
            _feedbackText.text = msg;
            _feedbackText.color = color;
            _feedbackText.gameObject.SetActive(true);
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(HideFeedbackAfter(2f));
        }

        private IEnumerator HideFeedbackAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_feedbackText != null) _feedbackText.gameObject.SetActive(false);
            _feedbackCoroutine = null;
        }
    }
}
