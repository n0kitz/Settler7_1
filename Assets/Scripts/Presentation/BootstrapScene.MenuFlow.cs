using UnityEngine;
using Settlers.UI;

namespace Settlers.Presentation
{
    /// <summary>
    /// BootstrapScene — menu click handlers: main menu entries, campaign and
    /// map-editor flows, load-game, and the map-selection → setup → start chain.
    /// Scene construction and game wiring live in BootstrapScene.cs.
    /// </summary>
    public partial class BootstrapScene
    {
        private void OnNewGameClicked()
        {
            _mainMenu.Hide();
            _mapSelect.Show();
        }

        private void OnTutorialClicked()
        {
            _mainMenu.Hide();
            // Start tutorial map directly — no map/setup screens
            StartTrackedGame("tutorial", playerCount: 1, vpRequired: 3);
        }

        private void OnCampaignClicked()
        {
            _mainMenu.Hide();
            _campaignProgress ??= Simulation.CampaignProgress.Load();
            _campaignSelect?.Show(_campaignProgress);
        }

        private void OnCampaignMissionSelected(Simulation.Mission mission)
        {
            _pendingMission = mission;
            _campaignSelect?.Hide();
            _missionBriefing?.Show(mission);
        }

        private void OnMissionStart(Simulation.Mission mission)
        {
            StartTrackedGame(mission.MapId, mission.PlayerCount, mission.VPRequired);
            WireCampaign(mission);
        }

        private void OnMissionBriefingBack()
        {
            _missionBriefing?.Hide();
            _campaignSelect?.Show(_campaignProgress);
        }

        private void OnCampaignBack()
        {
            _campaignSelect?.Hide();
            _mainMenu.Show();
        }

        private void OnMapEditorClicked()
        {
            _mainMenu.Hide();
            if (_mapEditorController == null)
            {
                var go = new GameObject("MapEditorController");
                _mapEditorController = go.AddComponent<MapEditorController>();
            }
            var editorUI = FindAnyObjectByType<UI.MapEditorUI>();
            var propPanel = FindAnyObjectByType<UI.SectorPropertyPanel>();
            _mapEditorController.OnEditorClosed += OnMapEditorClosed;
            _mapEditorController.OnPlaytestRequested += OnMapEditorPlaytest;
            _mapEditorController.Activate(editorUI, propPanel);
        }

        private void OnMapEditorClosed()
        {
            if (_mapEditorController != null)
            {
                _mapEditorController.OnEditorClosed -= OnMapEditorClosed;
                _mapEditorController.OnPlaytestRequested -= OnMapEditorPlaytest;
            }
            _mainMenu.Show();
        }

        private void OnMapEditorPlaytest(Simulation.MapEditorState editorState)
        {
            if (GameController.Instance == null) return;
            var graph = editorState.ToSectorGraph();
            var rules = Simulation.GameRules.Default;
            // Inline-launch the editor map as a skirmish
            GameController.Instance.StartGame("editor_custom",
                editorState.MaxPlayers, editorState.DefaultVP,
                Simulation.AIDifficultyLevel.Normal,
                Simulation.AIPersonalityType.Builder, rules);
        }

        private void OnSettingsClicked() => _settingsUI?.Show();

        private void OnAchievementsClicked() => _achievementsPanel?.Show();
        private void OnHallOfFameClicked()   => _hallOfFame?.Show();

        private void OnLoadGameClicked()
        {
            _mainMenu.Hide();

            // Need a game initialized before loading — start with default map
            if (GameController.Instance != null && GameController.Instance.State == null)
                GameController.Instance.SetMapId("test_valley");

            _loadSlotUI.Show(SaveSlotUI.Mode.Load);
        }

        private void OnLoadSlotClosed()
        {
            // If closed without loading, return to main menu
            _mainMenu.Show();
        }

        private void OnQuitToMenu()
        {
            _mapSelect.Hide();
            _gameSetup.Hide();
            _mainMenu.Show();
        }

        private void OnMapSelected(string mapId)
        {
            _mapSelect.Hide();
            var mapInfo = Simulation.MapFactory.CreateMap(mapId);
            _gameSetup.SetMap(mapId, mapInfo.DisplayName, mapInfo.PlayerCount, mapInfo.VPRequired);
            _gameSetup.Show();
        }

        private void OnStartGame(string mapId, int playerCount, int vpRequired,
            Simulation.AIDifficultyLevel difficulty, Simulation.AIPersonalityType personality,
            Simulation.StartingProfileType startingProfile, Simulation.VictoryRuleSetType victoryRules)
        {
            var rules = new Simulation.GameRules(
                Simulation.StartingProfile.Get(startingProfile),
                Simulation.VictoryRuleSet.Get(victoryRules));
            StartTrackedGame(mapId, playerCount, vpRequired,
                difficulty, personality, rules);
        }

        private void OnGameSetupBack() => _mapSelect.Show();
    }
}
