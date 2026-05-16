using System.Text;
using UnityEngine;
using TMPro;
using Settlers.Simulation;

namespace Settlers.UI
{
    /// <summary>
    /// Shows active quests, available quests, and objectives for player 0.
    /// Toggle with Q key. Refreshes every second while visible.
    /// Partial: QuestPanel.Factory.cs holds Create() and layout builders.
    /// </summary>
    public partial class QuestPanel : MonoBehaviour
    {
        private Transform _activeContainer;
        private Transform _availableContainer;
        private TMP_FontAsset _font;

        public bool IsVisible { get; private set; }

        private float _refreshTimer;
        private const float REFRESH_INTERVAL = 1f;

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

        public void Show()  { gameObject.SetActive(true);  IsVisible = true; _refreshTimer = 0f; }
        public void Hide()  { gameObject.SetActive(false); IsVisible = false; }
        public void Toggle() { if (IsVisible) Hide(); else Show(); }

        // ---- Refresh ----------------------------------------------------

        private void Refresh()
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;
            RefreshActiveQuests(state);
            RefreshAvailableQuests(state);
        }

        private void RefreshActiveQuests(GameState state)
        {
            ClearContainer(_activeContainer);
            var active = state.Quests.GetActiveQuests(0);
            if (active.Count == 0)
            {
                var none = UIFactory.CreateLabel(_activeContainer, "None",
                    "No active quests.", 12, FontStyles.Normal, _font);
                none.color = UIColors.TEXT_GRAY_DIM;
                return;
            }
            foreach (var quest in active)
                BuildQuestEntry(_activeContainer, quest, state, isActive: true);
        }

        private void RefreshAvailableQuests(GameState state)
        {
            ClearContainer(_availableContainer);
            var available = state.Quests.AvailableQuests;
            if (available.Count == 0)
            {
                var none = UIFactory.CreateLabel(_availableContainer, "None",
                    "No available quests.", 12, FontStyles.Normal, _font);
                none.color = UIColors.TEXT_GRAY_DIM;
                return;
            }
            foreach (var quest in available)
            {
                if (state.Quests.IsCompleted(quest.Id)) continue;
                BuildQuestEntry(_availableContainer, quest, state, isActive: false);
            }
        }

        // ---- Button actions ---------------------------------------------

        private void AcceptQuest(string questId)
        {
            var state = Presentation.GameController.Instance?.State;
            if (state == null) return;
            state.Quests.AcceptQuest(0, questId);
            Refresh();
        }
    }
}
