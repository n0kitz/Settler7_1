using System.Text;
using UnityEngine;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Displays generals, unit compositions, training queue, and training controls.
    /// Toggle with M key. Refreshes every 0.5s while visible.
    /// </summary>
    public class ArmyPanel : MonoBehaviour
    {
        // Directly assigned by ArmyPanelFactory (same assembly — internal access)
        internal TMP_FontAsset _font;

        // Set via UIFactory.SetField reflection
        private GameObject _panelRoot;
        private TextMeshProUGUI _statusText;
        private Transform _generalsContainer;
        private Transform _trainingContainer;
        private TextMeshProUGUI _trainingQueueText;

        public bool IsVisible { get; private set; }

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;

        // ---- Lifecycle --------------------------------------------------

        private void Update()
        {
            if (!IsVisible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                Refresh();
            }
        }

        // ---- Public API -------------------------------------------------

        public void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            _refreshTimer = 0f; // Refresh immediately on show
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            IsVisible = false;
        }

        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        /// <summary>
        /// Train a unit for player 0 in their first owned sector.
        /// </summary>
        public void TrainUnit(UnitType unitType)
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;

            int sectorId = FindOwnedSectorId(state, 0);
            if (sectorId < 0)
            {
                SetFeedback("No owned sector available.");
                return;
            }

            bool ok = state.Army.TrainUnit(0, sectorId, unitType);
            SetFeedback(ok ? $"Training {unitType}..." : "Cannot train: missing prestige or resources.");
            if (ok) Refresh();
        }

        /// <summary>Populate all display data from current simulation state.</summary>
        public void Refresh()
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;

            var army = state.Army;
            var generals = army.GetGenerals(0);
            int totalSoldiers = army.GetTotalArmySize(0);

            if (_statusText != null)
                _statusText.text = $"Generals: {generals.Count}/5  |  Total Soldiers: {totalSoldiers}";

            RefreshGeneralsList(generals, state);
            RefreshTrainingQueue(army);
        }

        // ---- Factory entry point ----------------------------------------

        /// <summary>Create via ArmyPanelFactory. Called by BootstrapScene.UI.</summary>
        public static ArmyPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return ArmyPanelFactory.Create(canvasTransform, font);
        }

        // ---- Private helpers --------------------------------------------

        private void RefreshGeneralsList(
            System.Collections.Generic.IReadOnlyList<General> generals,
            GameState state)
        {
            if (_generalsContainer == null) return;

            // Clear existing rows (skip header child at index 0)
            for (int i = _generalsContainer.childCount - 1; i >= 1; i--)
                Destroy(_generalsContainer.GetChild(i).gameObject);

            if (generals.Count == 0)
            {
                var none = UIFactory.CreateLabel(_generalsContainer,
                    "NoGenerals", "No generals hired yet.", 12,
                    TMPro.FontStyles.Normal, _font);
                none.color = UIColors.TEXT_GRAY_DIM;
                return;
            }

            var sb = new StringBuilder();
            foreach (var gen in generals)
            {
                sb.Clear();
                sb.Append($"General #{gen.Id}  Sec:{gen.SectorId}");
                if (gen.IsMoving) sb.Append(" [Moving]");
                sb.Append($"\n  ATK:{gen.TotalAttack} DEF:{gen.TotalDefense}");
                sb.Append($"  ({gen.TotalSoldiers}/{gen.MaxSoldiers} soldiers)");

                var row = UIFactory.CreateLabel(_generalsContainer,
                    $"Gen{gen.Id}", sb.ToString(), 11,
                    TMPro.FontStyles.Normal, _font);
                row.color = gen.IsMoving ? UIColors.ACCENT_ORANGE : UIColors.TEXT_GOLD;
                var le = row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                le.preferredHeight = 36f;
            }
        }

        private void RefreshTrainingQueue(ArmySystem army)
        {
            if (_trainingQueueText == null) return;

            var queue = army.TrainingQueue;
            if (queue.Count == 0)
            {
                _trainingQueueText.text = "No units training.";
                _trainingQueueText.color = UIColors.TEXT_GRAY_DIM;
                return;
            }

            var sb = new StringBuilder();
            foreach (var task in queue)
                sb.AppendLine($"{task.UnitType}  {(int)(task.Progress * 100f)}%");

            _trainingQueueText.text = sb.ToString().TrimEnd();
            _trainingQueueText.color = UIColors.TEXT_GOLD;
        }

        private void SetFeedback(string msg)
        {
            if (_statusText != null)
            {
                _statusText.text = msg;
                _statusText.color = msg.StartsWith("Cannot") ? UIColors.ACCENT_ORANGE : UIColors.TEXT_GOLD;
                // Reset after 2s
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), 2f);
            }
        }

        private void ClearFeedback() => Refresh();

        private static int FindOwnedSectorId(GameState state, int playerId)
        {
            foreach (var sector in state.Graph.GetAllSectors())
                if (sector.OwnerId == playerId) return sector.Id;
            return -1;
        }
    }
}
