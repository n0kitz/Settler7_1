using System;
using System.IO;
using UnityEngine;
using Settlers.Simulation;
using Settlers.UI;

namespace Settlers.Presentation
{
    /// <summary>
    /// Orchestrates the in-game map editor: handles tool state, click-to-place,
    /// edge drawing, serialization, and launching playtests.
    /// Activated by MainMenuUI → "Map Editor" button.
    /// </summary>
    public class MapEditorController : MonoBehaviour
    {
        public enum Tool { None, AddSector, DrawRoad, Delete }

        private MapEditorState _editorState;
        private Tool _activeTool = Tool.None;
        private int _roadStartId = -1;
        private MapEditorState.EditorSector _selectedSector;

        private MapEditorUI _editorUI;
        private SectorPropertyPanel _propertyPanel;

        public event Action<MapEditorState> OnPlaytestRequested;
        public event Action OnEditorClosed;

        public static MapEditorController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _editorState = new MapEditorState();
        }

        public void Activate(MapEditorUI editorUI, SectorPropertyPanel propertyPanel)
        {
            _editorUI = editorUI;
            _propertyPanel = propertyPanel;

            if (_editorUI != null)
            {
                _editorUI.OnAddSector += () => SetTool(Tool.AddSector);
                _editorUI.OnDrawRoad += () => SetTool(Tool.DrawRoad);
                _editorUI.OnDeleteTool += () => SetTool(Tool.Delete);
                _editorUI.OnPlaytest += RequestPlaytest;
                _editorUI.OnSave += SaveMap;
                _editorUI.OnLoad += LoadMap;
                _editorUI.OnClose += CloseEditor;
                _editorUI.Show();
            }

            if (_propertyPanel != null)
            {
                _propertyPanel.OnGarrisonChanged += delta =>
                {
                    if (_selectedSector != null)
                    {
                        _selectedSector.GarrisonStrength =
                            Mathf.Max(0, _selectedSector.GarrisonStrength + delta);
                        _propertyPanel.Refresh();
                    }
                };
                _propertyPanel.OnSlotsChanged += delta =>
                {
                    if (_selectedSector != null)
                    {
                        _selectedSector.BuildSlots =
                            Mathf.Clamp(_selectedSector.BuildSlots + delta, 1, 12);
                        _propertyPanel.Refresh();
                    }
                };
                _propertyPanel.OnDeleteSector += DeleteSelected;
            }

            RefreshStatus();
        }

        public void HandleEditorClick(Vector3 worldPos, int clickedSectorId = -1)
        {
            switch (_activeTool)
            {
                case Tool.AddSector:
                    var s = _editorState.AddSector(worldPos.x, worldPos.z);
                    SelectSector(s);
                    RefreshStatus();
                    break;

                case Tool.DrawRoad:
                    if (clickedSectorId < 0) break;
                    if (_roadStartId < 0)
                    {
                        _roadStartId = clickedSectorId;
                        _editorUI?.SetStatus($"Road start: sector {_roadStartId}. Click a second sector.");
                    }
                    else
                    {
                        bool added = _editorState.AddEdge(_roadStartId, clickedSectorId);
                        _editorUI?.SetStatus(added
                            ? $"Road added: {_roadStartId} — {clickedSectorId}"
                            : "Road already exists.");
                        _roadStartId = -1;
                    }
                    break;

                case Tool.Delete:
                    if (clickedSectorId >= 0)
                    {
                        _editorState.RemoveSector(clickedSectorId);
                        if (_selectedSector?.Id == clickedSectorId)
                        {
                            _selectedSector = null;
                            _propertyPanel?.Hide();
                        }
                        RefreshStatus();
                    }
                    break;

                default:
                    if (clickedSectorId >= 0)
                        SelectSector(_editorState.FindSector(clickedSectorId));
                    break;
            }
        }

        private void SetTool(Tool tool)
        {
            _activeTool = tool;
            _roadStartId = -1;
            RefreshStatus();
        }

        private void SelectSector(MapEditorState.EditorSector sector)
        {
            _selectedSector = sector;
            if (sector != null && _propertyPanel != null)
                _propertyPanel.Bind(sector);
            else
                _propertyPanel?.Hide();
        }

        private void DeleteSelected()
        {
            if (_selectedSector == null) return;
            _editorState.RemoveSector(_selectedSector.Id);
            _selectedSector = null;
            _propertyPanel?.Hide();
            RefreshStatus();
        }

        private void RequestPlaytest()
        {
            var errors = MapValidation.Validate(_editorState);
            if (errors.Count > 0)
            {
                _editorUI?.SetStatus("Cannot playtest: " + string.Join("; ", errors));
                return;
            }
            OnPlaytestRequested?.Invoke(_editorState);
        }

        private void SaveMap()
        {
            string json = MapSerializer.Serialize(_editorState);
            string path = GetSavePath();
            File.WriteAllText(path, json);
            _editorUI?.SetStatus($"Saved to {path}");
        }

        private void LoadMap()
        {
            string path = GetSavePath();
            if (!File.Exists(path))
            {
                _editorUI?.SetStatus("No saved map found.");
                return;
            }
            string json = File.ReadAllText(path);
            var loaded = MapSerializer.Deserialize(json, out string error);
            if (loaded == null)
            {
                _editorUI?.SetStatus($"Load failed: {error}");
                return;
            }
            _editorState = loaded;
            _selectedSector = null;
            _propertyPanel?.Hide();
            _editorUI?.SetMapName(_editorState.MapName);
            RefreshStatus();
        }

        private void CloseEditor()
        {
            _editorUI?.Hide();
            _propertyPanel?.Hide();
            OnEditorClosed?.Invoke();
        }

        private void RefreshStatus()
        {
            string tool = _activeTool switch
            {
                Tool.AddSector => "PLACE SECTOR — click to add.",
                Tool.DrawRoad  => "DRAW ROAD — click two sectors.",
                Tool.Delete    => "DELETE — click a sector to remove.",
                _              => $"Sectors: {_editorState.Sectors.Count}  Roads: {_editorState.Edges.Count}. Select a tool above."
            };
            _editorUI?.SetStatus(tool);
        }

        private static string GetSavePath()
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Settlers7", "Maps");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "custom_map.json");
        }
    }
}
