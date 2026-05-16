using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Lists all scenarios (built-in hardcoded + mod-provided) and lets the player
    /// launch one. Opened from MainMenuUI "Custom Scenarios" button.
    /// </summary>
    public class ScenarioSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform  _listContainer;

        /// <summary>Fired when the player picks a scenario to start.</summary>
        public event System.Action<ScenarioDefinition> OnScenarioSelected;

        public void Show()
        {
            if (_panelRoot) _panelRoot.SetActive(true);
            Refresh();
        }

        public void Hide() { if (_panelRoot) _panelRoot.SetActive(false); }

        private void Refresh()
        {
            ClearList();
            var all = CollectScenarios();
            foreach (var sc in all)
                AddRow(sc);
        }

        private List<ScenarioDefinition> CollectScenarios()
        {
            var list = new List<ScenarioDefinition>();

            // Built-in scenarios
            list.Add(new ScenarioDefinition
            {
                ScenarioId   = "blitz",
                DisplayName  = "Blitz Match",
                Description  = "Rich start, Conquest Only, Hard AI. Fast and brutal.",
                MapId        = "test_valley",
                PlayerCount  = 2,
                VPRequired   = 3,
                StartingProfile = "Rich",
                VictoryRules    = "ConquestOnly",
                AIDifficulty    = "Hard",
                AIPersonality   = "Warrior",
            });
            list.Add(new ScenarioDefinition
            {
                ScenarioId   = "trade_race",
                DisplayName  = "Trade Race",
                Description  = "Lean start, Trade VPs only. Economy wins the game.",
                MapId        = "test_valley",
                PlayerCount  = 2,
                VPRequired   = 5,
                StartingProfile = "Lean",
                VictoryRules    = "TradeOnly",
                AIPersonality   = "Merchant",
            });

            // Mod scenarios
            foreach (var mod in ModLoader.Loaded)
            {
                var scenariosDir = System.IO.Path.Combine(mod.RootPath, "Scenarios");
                if (!System.IO.Directory.Exists(scenariosDir)) continue;
                foreach (var file in System.IO.Directory.GetFiles(scenariosDir, "*.scenario.json"))
                {
                    try
                    {
                        var sc = ParseScenarioFile(file, mod.ModId);
                        if (sc != null) list.Add(sc);
                    }
                    catch (System.Exception) { }
                }
            }
            return list;
        }

        private static ScenarioDefinition ParseScenarioFile(string path, string modId)
        {
            var sc = new ScenarioDefinition { IsModScenario = true, ModId = modId };
            foreach (var raw in System.IO.File.ReadAllLines(path))
            {
                var line = raw.Trim();
                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string key = line.Substring(0, colon).Trim().Trim('"');
                string val = line.Substring(colon + 1).Trim().Trim(',').Trim('"');
                switch (key)
                {
                    case "scenarioId":   sc.ScenarioId   = val; break;
                    case "displayName":  sc.DisplayName  = val; break;
                    case "description":  sc.Description  = val; break;
                    case "mapId":        sc.MapId        = val; break;
                    case "playerCount":  int.TryParse(val, out int pc); sc.PlayerCount = pc; break;
                    case "vpRequired":   int.TryParse(val, out int vp); sc.VPRequired  = vp; break;
                    case "startingProfile": sc.StartingProfile = val; break;
                    case "victoryRules":    sc.VictoryRules    = val; break;
                    case "aiPersonality":   sc.AIPersonality   = val; break;
                    case "aiDifficulty":    sc.AIDifficulty    = val; break;
                }
            }
            return string.IsNullOrEmpty(sc.ScenarioId) ? null : sc;
        }

        private void AddRow(ScenarioDefinition sc)
        {
            var font = UIFactory.GetDefaultFont();
            var row = new GameObject($"ScenRow_{sc.ScenarioId}");
            row.transform.SetParent(_listContainer, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 70f);
            row.AddComponent<Image>().color = UIColors.PANEL_GRAY_MEDIUM;
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding  = new RectOffset(10, 10, 8, 8);
            layout.spacing  = 10f;
            layout.childForceExpandWidth  = false;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>().preferredHeight = 70f;

            var info = new GameObject("Info");
            info.transform.SetParent(row.transform, false);
            info.AddComponent<VerticalLayoutGroup>().childForceExpandWidth = true;
            info.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var nameLabel = UIFactory.CreateLabel(info.transform, "Name",
                sc.DisplayName, 15f, FontStyles.Bold, font);
            nameLabel.color = UIColors.TEXT_HEADER_GOLD;
            var descLabel = UIFactory.CreateLabel(info.transform, "Desc",
                sc.Description, 11f, font);
            descLabel.color = UIColors.TEXT_GRAY_DIM;
            if (sc.IsModScenario)
            {
                var modLabel = UIFactory.CreateLabel(info.transform, "Mod",
                    $"[Mod: {sc.ModId}]", 10f, font);
                modLabel.color = UIColors.ACCENT_ORANGE;
            }

            var captured = sc;
            UIFactory.CreateButton(row.transform, "Play", font,
                UIColors.BUTTON_GREEN,
                () => { Hide(); OnScenarioSelected?.Invoke(captured); },
                new Vector2(70f, 40f), 16f);
        }

        private void ClearList()
        {
            if (_listContainer == null) return;
            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);
        }

        private void OnCloseClicked() { Hide(); }

        public static ScenarioSelectionUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            var panelGo = new GameObject("ScenarioSelectionUI");
            panelGo.transform.SetParent(canvasTransform, false);

            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.1f);
            panelRect.anchorMax = new Vector2(0.8f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            panelGo.AddComponent<Image>().color = UIColors.PANEL_BLUE_DARK;

            var outerLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            outerLayout.padding = new RectOffset(14, 14, 14, 14);
            outerLayout.spacing = 10f;
            outerLayout.childForceExpandWidth  = true;
            outerLayout.childForceExpandHeight = false;

            var title = UIFactory.CreateLabel(panelGo.transform, "Title",
                "Custom Scenarios", 22f, FontStyles.Bold, font);
            title.color = UIColors.TEXT_HEADER_GOLD;
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;

            var list = new GameObject("List");
            list.transform.SetParent(panelGo.transform, false);
            list.AddComponent<RectTransform>();
            var listLayout = list.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 6f;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;
            list.AddComponent<LayoutElement>().flexibleHeight = 1f;

            var ui = panelGo.AddComponent<ScenarioSelectionUI>();
            UIFactory.SetField(ui, "_panelRoot",     panelGo);
            UIFactory.SetField(ui, "_listContainer", list.transform);

            UIFactory.CreateButton(panelGo.transform, "Close", font,
                UIColors.BUTTON_RED, ui.OnCloseClicked, new Vector2(120f, 40f), 16f);

            panelGo.SetActive(false);
            return ui;
        }
    }
}
