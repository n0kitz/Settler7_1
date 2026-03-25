using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Shows generals, army composition, training queue, and unit training buttons.
    /// Toggle with M key.
    /// </summary>
    public class ArmyPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Transform _generalsContainer;
        [SerializeField] private Transform _trainingContainer;
        [SerializeField] private TextMeshProUGUI _trainingQueueText;

        internal TMP_FontAsset _font;
        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.5f;
        private readonly List<GameObject> _generalRows = new();

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            RefreshNow();
        }

        public void Hide() { if (_panelRoot != null) _panelRoot.SetActive(false); }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        private void Update()
        {
            if (!IsVisible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                RefreshNow();
            }
        }

        private void RefreshNow()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var army = gc.State.Army;
            var generals = army.GetGenerals(playerId);

            if (_statusText != null)
            {
                int total = army.GetTotalArmySize(playerId);
                _statusText.text = $"Generals: {generals.Count}/5  |  Total Soldiers: {total}";
            }

            foreach (var go in _generalRows)
                if (go != null) Destroy(go);
            _generalRows.Clear();

            foreach (var gen in generals)
                _generalRows.Add(CreateGeneralRow(gen));

            if (_trainingQueueText != null)
            {
                var queue = army.TrainingQueue;
                if (queue.Count == 0)
                {
                    _trainingQueueText.text = "No units training.";
                }
                else
                {
                    string txt = "";
                    foreach (var task in queue)
                    {
                        if (task.PlayerId != playerId) continue;
                        int pct = (int)(task.Progress / task.TotalTime * 100f);
                        txt += $"{task.UnitType} — {pct}%\n";
                    }
                    _trainingQueueText.text = txt.Length > 0 ? txt.TrimEnd() : "No units training.";
                }
            }
        }

        private GameObject CreateGeneralRow(General gen)
        {
            var rowGo = new GameObject($"General_{gen.Id}");
            rowGo.transform.SetParent(_generalsContainer, false);

            rowGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 60f);
            rowGo.AddComponent<LayoutElement>().preferredHeight = 60f;

            var bg = rowGo.AddComponent<Image>();
            bg.color = gen.IsMoving
                ? new Color(0.15f, 0.15f, 0.25f, 0.8f)
                : UIColors.PANEL_GRAY_MEDIUM;

            string moving = gen.IsMoving ? " [MOVING]" : "";
            string header = $"General #{gen.Id}  —  Sector {gen.SectorId}{moving}  " +
                            $"({gen.TotalSoldiers}/{gen.MaxSoldiers})";

            var headerText = UIFactory.CreateLabel(rowGo.transform, "Header", header,
                13, FontStyles.Bold, _font);
            var hRect = headerText.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 0.5f);
            hRect.anchorMax = new Vector2(1f, 1f);
            hRect.offsetMin = new Vector2(8f, 0f);
            hRect.offsetMax = new Vector2(-8f, -2f);
            headerText.alignment = TextAlignmentOptions.MidlineLeft;

            string units = "";
            foreach (var kvp in gen.Units)
                if (kvp.Value > 0) units += $"{kvp.Key}:{kvp.Value}  ";
            if (units.Length == 0) units = "(empty)";

            var unitsText = UIFactory.CreateLabel(rowGo.transform, "Units", units.TrimEnd(),
                11, FontStyles.Normal, _font);
            unitsText.color = UIColors.TEXT_GRAY_DIM;
            var uRect = unitsText.GetComponent<RectTransform>();
            uRect.anchorMin = new Vector2(0f, 0f);
            uRect.anchorMax = new Vector2(1f, 0.5f);
            uRect.offsetMin = new Vector2(8f, 2f);
            uRect.offsetMax = new Vector2(-8f, 0f);
            unitsText.alignment = TextAlignmentOptions.MidlineLeft;

            var statsText = UIFactory.CreateLabel(rowGo.transform, "Stats",
                $"ATK:{gen.TotalAttack}  DEF:{gen.TotalDefense}", 11, FontStyles.Normal, _font);
            statsText.color = UIColors.TEXT_GOLD;
            statsText.alignment = TextAlignmentOptions.MidlineRight;
            var sRect = statsText.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.7f, 0.5f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.offsetMin = new Vector2(0f, 0f);
            sRect.offsetMax = new Vector2(-8f, -2f);

            return rowGo;
        }

        internal void TrainUnit(UnitType unitType)
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            var sectors = gc.State.Graph.GetSectorsOwnedBy(0);
            if (sectors.Count == 0) return;

            gc.State.Army.TrainUnit(0, sectors[0].Id, unitType);
            RefreshNow();
        }

        public static ArmyPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return ArmyPanelFactory.Create(canvasTransform, font);
        }
    }
}
