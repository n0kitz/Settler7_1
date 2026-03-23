using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Army management panel. Shows generals, their units, training queue.
    /// Allows hiring generals, training units, and sending armies.
    /// Toggle with M key.
    /// </summary>
    public class ArmyPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private Transform _generalsContainer;
        [SerializeField] private Transform _trainingContainer;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        private bool _isVisible;
        private readonly List<GameObject> _dynamicElements = new();
        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 0.25f;

        public void Show()
        {
            _isVisible = true;
            if (_panelRoot != null)
                _panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            _isVisible = false;
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        public void Toggle()
        {
            if (_isVisible) Hide();
            else Show();
        }

        public bool IsVisible => _isVisible;

        private void Update()
        {
            if (!_isVisible) return;
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = REFRESH_INTERVAL;
                Refresh();
            }
        }

        private void Refresh()
        {
            var gc = Presentation.GameController.Instance;
            if (gc == null || gc.State == null) return;

            int playerId = 0;
            var army = gc.State.Army;
            var generals = army.GetGenerals(playerId);

            // Summary
            if (_summaryText != null)
            {
                int totalArmy = army.GetTotalArmySize(playerId);
                _summaryText.text = $"Generals: {generals.Count}/5  Total Army: {totalArmy}";
            }

            // Clear dynamic elements
            foreach (var go in _dynamicElements)
                if (go != null) Destroy(go);
            _dynamicElements.Clear();

            // Show each general
            if (_generalsContainer != null)
            {
                foreach (var gen in generals)
                {
                    var label = CreateDynamicLabel(_generalsContainer,
                        $"General #{gen.Id} @ Sector {gen.SectorId}  " +
                        $"[{gen.TotalSoldiers}/{gen.MaxSoldiers}]" +
                        (gen.IsMoving ? " (Moving)" : ""));
                    _dynamicElements.Add(label);

                    // Unit breakdown
                    foreach (var kvp in gen.Units)
                    {
                        if (kvp.Value > 0)
                        {
                            var unitLabel = CreateDynamicLabel(_generalsContainer,
                                $"   {kvp.Key}: {kvp.Value}");
                            unitLabel.GetComponent<TextMeshProUGUI>().fontSize = 11;
                            unitLabel.GetComponent<TextMeshProUGUI>().color =
                                new Color(0.7f, 0.8f, 0.7f);
                            _dynamicElements.Add(unitLabel);
                        }
                    }
                }

                if (generals.Count == 0)
                {
                    var noGen = CreateDynamicLabel(_generalsContainer,
                        "No generals. Hire one at the Tavern.");
                    noGen.GetComponent<TextMeshProUGUI>().color =
                        new Color(0.6f, 0.6f, 0.6f);
                    _dynamicElements.Add(noGen);
                }
            }

            // Show training queue
            if (_trainingContainer != null)
            {
                var tasks = army.TrainingQueue;
                foreach (var task in tasks)
                {
                    if (task.PlayerId != playerId) continue;
                    int pct = (int)(task.Progress * 100);
                    var label = CreateDynamicLabel(_trainingContainer,
                        $"Training {task.UnitType} [{pct}%]");
                    _dynamicElements.Add(label);
                }

                if (tasks.Count == 0)
                {
                    var noTrain = CreateDynamicLabel(_trainingContainer,
                        "No units in training.");
                    noTrain.GetComponent<TextMeshProUGUI>().color =
                        new Color(0.6f, 0.6f, 0.6f);
                    _dynamicElements.Add(noTrain);
                }
            }
        }

        private GameObject CreateDynamicLabel(Transform parent, string text)
        {
            var go = new GameObject("DynLabel");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 18f);

            var layoutElem = go.AddComponent<LayoutElement>();
            layoutElem.preferredHeight = 18f;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            var font = UIFactory.GetDefaultFont();
            if (font != null) tmp.font = font;

            return go;
        }

        /// <summary>Create the army panel UI programmatically.</summary>
        public static ArmyPanel Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("ArmyPanel");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.1f);
            panelRect.anchorMax = new Vector2(0.85f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.06f, 0.1f, 0.95f);

            // Title
            var titleText = CreateLabel(panelGo.transform, "Title",
                "Army Management", 20, FontStyles.Bold, font);
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(0f, 28f);
            titleText.alignment = TextAlignmentOptions.Center;

            // Summary
            var summaryText = CreateLabel(panelGo.transform, "Summary",
                "Generals: 0/5  Total Army: 0", 14, FontStyles.Normal, font);
            summaryText.color = new Color(0.9f, 0.8f, 0.5f);
            var summaryRect = summaryText.GetComponent<RectTransform>();
            summaryRect.anchorMin = new Vector2(0f, 1f);
            summaryRect.anchorMax = new Vector2(1f, 1f);
            summaryRect.pivot = new Vector2(0.5f, 1f);
            summaryRect.anchoredPosition = new Vector2(0f, -38f);
            summaryRect.sizeDelta = new Vector2(0f, 20f);
            summaryText.alignment = TextAlignmentOptions.Center;

            // Two-column layout
            var columnsRoot = new GameObject("Columns");
            columnsRoot.transform.SetParent(panelGo.transform, false);
            var columnsRect = columnsRoot.AddComponent<RectTransform>();
            columnsRect.anchorMin = new Vector2(0f, 0f);
            columnsRect.anchorMax = new Vector2(1f, 1f);
            columnsRect.offsetMin = new Vector2(10f, 10f);
            columnsRect.offsetMax = new Vector2(-10f, -65f);

            var columnsLayout = columnsRoot.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.spacing = 10f;
            columnsLayout.childForceExpandWidth = true;
            columnsLayout.childForceExpandHeight = true;
            columnsLayout.padding = new RectOffset(5, 5, 5, 5);

            var generalsCol = CreateScrollColumn(columnsRoot.transform, "Generals", font);
            var trainingCol = CreateScrollColumn(columnsRoot.transform, "Training Queue", font);

            // Component
            var panel = panelGo.AddComponent<ArmyPanel>();
            SetField(panel, "_panelRoot", panelGo);
            SetField(panel, "_titleText", titleText);
            SetField(panel, "_summaryText", summaryText);
            SetField(panel, "_generalsContainer", generalsCol.transform);
            SetField(panel, "_trainingContainer", trainingCol.transform);

            panelGo.SetActive(false);
            return panel;
        }

        private static GameObject CreateScrollColumn(Transform parent, string label,
            TMP_FontAsset font)
        {
            var colGo = new GameObject($"Col_{label}");
            colGo.transform.SetParent(parent, false);
            colGo.AddComponent<RectTransform>();

            var colBg = colGo.AddComponent<Image>();
            colBg.color = new Color(0.12f, 0.12f, 0.15f, 0.8f);

            var colLayout = colGo.AddComponent<VerticalLayoutGroup>();
            colLayout.padding = new RectOffset(6, 6, 6, 6);
            colLayout.spacing = 3f;
            colLayout.childForceExpandWidth = true;
            colLayout.childForceExpandHeight = false;
            colLayout.childAlignment = TextAnchor.UpperLeft;

            var headerText = CreateLabel(colGo.transform, "Header", label, 15,
                FontStyles.Bold, font);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = new Color(0.9f, 0.7f, 0.4f);

            return colGo;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name,
            string text, float fontSize, FontStyles style, TMP_FontAsset font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize + 6f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            if (font != null) tmp.font = font;

            return tmp;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
