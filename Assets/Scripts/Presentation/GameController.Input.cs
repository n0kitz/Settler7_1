using UnityEngine;
using UnityEngine.InputSystem;

namespace Settlers.Presentation
{
    public partial class GameController
    {
        /// <summary>True if ESC was consumed this frame (prevents PauseMenuUI from double-firing).</summary>
        public static bool EscConsumedThisFrame { get; private set; }

        private void HandleKeyboardToggles()
        {
            if (Keyboard.current == null) return;

            EscConsumedThisFrame = false;

            // ESC closes the first open panel (priority order)
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_buildingPlacer != null && _buildingPlacer.IsPlacing)
                    { _buildingPlacer.CancelPlacement(); EscConsumedThisFrame = true; return; }
                if (_techTreeUI != null && _techTreeUI.IsVisible)
                    { _techTreeUI.Hide(); EscConsumedThisFrame = true; return; }
                if (_tradeMapUI != null && _tradeMapUI.IsVisible)
                    { _tradeMapUI.Hide(); EscConsumedThisFrame = true; return; }
                if (_armyPanel != null && _armyPanel.IsVisible)
                    { _armyPanel.Hide(); EscConsumedThisFrame = true; return; }
                if (_prestigeChart != null && _prestigeChart.IsVisible)
                    { _prestigeChart.Hide(); EscConsumedThisFrame = true; return; }
                if (_tavernUI != null && _tavernUI.IsVisible)
                    { _tavernUI.Hide(); EscConsumedThisFrame = true; return; }
                if (_buildMenu != null && _buildMenu.IsVisible)
                    { _buildMenu.Hide(); EscConsumedThisFrame = true; return; }
                if (_sectorPanel != null && _sectorPanel.IsVisible)
                    { _sectorPanel.Hide(); EscConsumedThisFrame = true; return; }
            }

            if (Keyboard.current.bKey.wasPressedThisFrame && _buildMenu != null)
                _buildMenu.Toggle();

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (_prestigeChart == null)
                    _prestigeChart = FindAnyObjectByType<PrestigeChartUI>();
                _prestigeChart?.Toggle();
            }

            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (_techTreeUI == null)
                    _techTreeUI = FindAnyObjectByType<TechTreeUI>();
                _techTreeUI?.Toggle();
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (_tradeMapUI == null)
                    _tradeMapUI = FindAnyObjectByType<TradeMapUI>();
                _tradeMapUI?.Toggle();
            }

            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                if (_armyPanel == null)
                    _armyPanel = FindAnyObjectByType<ArmyPanel>();
                _armyPanel?.Toggle();
            }

            if (Keyboard.current.vKey.wasPressedThisFrame)
            {
                if (_tavernUI == null)
                    _tavernUI = FindAnyObjectByType<TavernUI>();
                _tavernUI?.Toggle();
            }

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                if (_questPanel == null)
                    _questPanel = FindAnyObjectByType<UI.QuestPanel>();
                _questPanel?.Toggle();
            }

            // Game speed: 1/2/3 keys
            if (Keyboard.current.digit1Key.wasPressedThisFrame) Time.timeScale = 1f;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Time.timeScale = 2f;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) Time.timeScale = 4f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                Time.timeScale = Time.timeScale > 0f ? 0f : 1f; // Pause/unpause
        }
    }
}
