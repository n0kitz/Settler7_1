using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// ÜBERSICHT production stats panel (§14.1). Pick a good on the left,
    /// see the four verified columns: ERFORDERT (what its production needs),
    /// PRODUZIERT VON (which work yards make it), ERBRINGT (what it turns
    /// into), VERBRAUCHT VON (which work yards consume it). Data comes
    /// straight from RecipeDatabase.
    /// </summary>
    public class StatsOverviewUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private Transform _requiresCol;
        [SerializeField] private Transform _producedByCol;
        [SerializeField] private Transform _yieldsCol;
        [SerializeField] private Transform _consumedByCol;

        internal TMP_FontAsset PanelFont;
        internal readonly Dictionary<ResourceType, Image> _selectorImages = new();

        private ResourceType _selected = ResourceType.Planks;

        public bool IsVisible => _panelRoot != null && _panelRoot.activeSelf;

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            if (_titleText != null) _titleText.text = L.Get("ui.stats.title");

            // Column headers + selector labels live-switch with the locale
            RefreshHeader(_requiresCol, "ui.stats.col.requires");
            RefreshHeader(_producedByCol, "ui.stats.col.produced_by");
            RefreshHeader(_yieldsCol, "ui.stats.col.yields");
            RefreshHeader(_consumedByCol, "ui.stats.col.consumed_by");
            foreach (var kvp in _selectorImages)
            {
                if (kvp.Value == null) continue;
                var label = kvp.Value.transform.Find("Label");
                if (label != null && label.TryGetComponent<TextMeshProUGUI>(out var tmp))
                    tmp.text = LocalizedNames.Resource(kvp.Key);
            }

            SelectResource(_selected);
        }

        private static void RefreshHeader(Transform column, string key)
        {
            if (column == null || column.childCount == 0) return;
            if (column.GetChild(0).TryGetComponent<TextMeshProUGUI>(out var tmp))
                tmp.text = L.Get(key);
        }

        public void Hide() { if (_panelRoot != null) _panelRoot.SetActive(false); }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        /// <summary>Rebuild all four columns for the given good.</summary>
        public void SelectResource(ResourceType resource)
        {
            _selected = resource;

            foreach (var kvp in _selectorImages)
            {
                if (kvp.Value == null) continue;
                kvp.Value.color = kvp.Key == resource
                    ? UIColors.BORDER_GOLD : UIColors.TILE_BG;
            }

            if (_subtitleText != null)
                _subtitleText.text = string.Format(L.Get("ui.stats.production"),
                    LocalizedNames.Resource(resource));

            var requires = new List<string>();
            var producedBy = new List<string>();
            var yields = new List<string>();
            var consumedBy = new List<string>();
            var seen = new HashSet<string>();

            foreach (var recipe in RecipeDatabase.All)
            {
                if (ContainsResource(recipe.Outputs, resource))
                {
                    producedBy.Add(recipe.DisplayName);
                    foreach (var (type, amount) in recipe.Inputs)
                        AddUnique(requires, seen, $"req|{type}|{amount}",
                            $"{amount} × {LocalizedNames.Resource(type)}");
                }
                if (ContainsResource(recipe.Inputs, resource))
                {
                    consumedBy.Add(recipe.DisplayName);
                    foreach (var (type, amount) in recipe.Outputs)
                        AddUnique(yields, seen, $"yld|{type}|{amount}",
                            $"{amount} × {LocalizedNames.Resource(type)}");
                }
            }

            FillColumn(_requiresCol, requires);
            FillColumn(_producedByCol, producedBy);
            FillColumn(_yieldsCol, yields);
            FillColumn(_consumedByCol, consumedBy);
        }

        private static bool ContainsResource((ResourceType type, int amount)[] list,
            ResourceType resource)
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i].type == resource) return true;
            return false;
        }

        private static void AddUnique(List<string> target, HashSet<string> seen,
            string key, string entry)
        {
            if (seen.Add(key)) target.Add(entry);
        }

        /// <summary>Replace all entries below the header (child 0).</summary>
        private void FillColumn(Transform column, List<string> entries)
        {
            if (column == null) return;

            for (int i = column.childCount - 1; i >= 1; i--)
                Destroy(column.GetChild(i).gameObject);

            if (entries.Count == 0)
                entries.Add("—");

            foreach (var entry in entries)
            {
                var label = UIFactory.CreateLabel(column, "Entry", entry, 12, PanelFont);
                label.color = UIColors.TEXT_LIGHT;
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.gameObject.AddComponent<LayoutElement>().preferredHeight = 18f;
            }
        }

        public static StatsOverviewUI Create(Transform canvasTransform, TMP_FontAsset font)
        {
            return StatsOverviewUIFactory.Create(canvasTransform, font);
        }
    }
}
