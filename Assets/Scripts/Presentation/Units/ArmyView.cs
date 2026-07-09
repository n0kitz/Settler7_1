using System.Collections.Generic;
using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Presentation
{
    /// <summary>
    /// Visual representation of a general and their army.
    /// Shows the player banner with a plumed general figure beside it and a
    /// squad of spear-carrying soldiers whose count grows with army size.
    /// Moves between sectors when the army is in transit.
    /// </summary>
    public class ArmyView : MonoBehaviour
    {
        private const int MAX_SQUAD_FIGURES = 6;

        private int _generalId;
        private int _ownerId;
        private MeshRenderer _flagRenderer;
        private MeshRenderer _baseRenderer;
        private MaterialPropertyBlock _propBlock;
        private TextMesh _countLabel;
        private Material _material;
        private Color _playerColor;
        private Transform _squadRoot;
        private int _squadSize = -1;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly Color[] PLAYER_COLORS = {
            new Color(0.2f, 0.5f, 0.9f),
            new Color(0.9f, 0.3f, 0.2f),
            new Color(0.2f, 0.8f, 0.3f),
            new Color(0.9f, 0.8f, 0.2f)
        };

        public int GeneralId => _generalId;

        public static ArmyView Create(Transform parent, General general, Material material)
        {
            var go = new GameObject($"Army_{general.Id}");
            go.transform.SetParent(parent, false);

            // Base (flat cylinder)
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseGo.name = "Base";
            baseGo.transform.SetParent(go.transform, false);
            baseGo.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
            baseGo.transform.localPosition = new Vector3(0f, 0.025f, 0f);
            var baseCol = baseGo.GetComponent<Collider>();
            if (baseCol != null) Object.Destroy(baseCol);

            // Flag pole (tall thin cube)
            var flagGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flagGo.name = "Flag";
            flagGo.transform.SetParent(go.transform, false);
            flagGo.transform.localScale = new Vector3(0.08f, 1.5f, 0.08f);
            flagGo.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            var flagCol = flagGo.GetComponent<Collider>();
            if (flagCol != null) Object.Destroy(flagCol);

            // Flag banner (small cube on top)
            var bannerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bannerGo.name = "Banner";
            bannerGo.transform.SetParent(go.transform, false);
            bannerGo.transform.localScale = new Vector3(0.5f, 0.3f, 0.05f);
            bannerGo.transform.localPosition = new Vector3(0.25f, 1.35f, 0f);
            var bannerCol = bannerGo.GetComponent<Collider>();
            if (bannerCol != null) Object.Destroy(bannerCol);

            // Count label
            var labelGo = new GameObject("Count");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            var textMesh = labelGo.AddComponent<TextMesh>();
            textMesh.characterSize = 0.15f;
            textMesh.fontSize = 40;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            var view = go.AddComponent<ArmyView>();
            view._generalId = general.Id;
            view._ownerId = general.OwnerId;
            view._flagRenderer = flagGo.GetComponent<MeshRenderer>();
            view._baseRenderer = bannerGo.GetComponent<MeshRenderer>();
            view._propBlock = new MaterialPropertyBlock();
            view._countLabel = textMesh;

            if (material != null)
            {
                flagGo.GetComponent<MeshRenderer>().sharedMaterial = material;
                bannerGo.GetComponent<MeshRenderer>().sharedMaterial = material;
                baseGo.GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            // Set player color
            Color playerColor = general.OwnerId < PLAYER_COLORS.Length
                ? PLAYER_COLORS[general.OwnerId] : Color.white;
            view.SetBannerColor(playerColor);
            view._material = material;
            view._playerColor = playerColor;

            // General figure beside the banner
            var figure = UnitFigureFactory.CreateFigure(go.transform,
                UnitFigureFactory.Role.General, playerColor, material);
            figure.transform.localPosition = new Vector3(0.45f, 0f, 0.35f);
            figure.transform.localScale = Vector3.one * 1.15f;

            // Squad root — soldier figures are rebuilt as the army grows
            var squadGo = new GameObject("Squad");
            squadGo.transform.SetParent(go.transform, false);
            view._squadRoot = squadGo.transform;

            return view;
        }

        private void SyncSquad(int totalSoldiers)
        {
            int desired = totalSoldiers <= 0 ? 0
                : Mathf.Clamp(1 + totalSoldiers / 10, 1, MAX_SQUAD_FIGURES);
            if (desired == _squadSize) return;
            _squadSize = desired;

            for (int i = _squadRoot.childCount - 1; i >= 0; i--)
                Destroy(_squadRoot.GetChild(i).gameObject);

            for (int i = 0; i < desired; i++)
            {
                var soldier = UnitFigureFactory.CreateFigure(_squadRoot,
                    UnitFigureFactory.Role.Soldier, _playerColor, _material);
                float angle = Mathf.PI * 2f * i / MAX_SQUAD_FIGURES;
                soldier.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.9f, 0f, Mathf.Sin(angle) * 0.9f - 0.5f);
                soldier.transform.localScale = Vector3.one * 0.85f;
            }
        }

        /// <summary>Update position and label from general state.</summary>
        public void UpdateFromGeneral(General general)
        {
            var gc = GameController.Instance;
            if (gc == null) return;

            // Position at sector
            var pos = gc.GetSectorPosition(general.SectorId);
            // Offset slightly so armies don't overlap buildings
            pos += new Vector3(2f, 0f, 2f);
            transform.position = pos;

            // Update count label
            if (_countLabel != null)
                _countLabel.text = general.TotalSoldiers.ToString();

            SyncSquad(general.TotalSoldiers);

            // Make label face camera
            if (Camera.main != null && _countLabel != null)
                _countLabel.transform.rotation = Camera.main.transform.rotation;
        }

        private void SetBannerColor(Color color)
        {
            if (_baseRenderer == null) return;
            _baseRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(ColorProp, color);
            _baseRenderer.SetPropertyBlock(_propBlock);
        }
    }

    /// <summary>
    /// Manages ArmyView instances, syncing with ArmySystem generals.
    /// </summary>
    public class ArmyViewManager : MonoBehaviour
    {
        private readonly Dictionary<int, ArmyView> _views = new();
        private readonly List<int> _toRemove = new();
        private Transform _root;
        private Material _material;

        public void Initialize(Transform root, Material material)
        {
            _root = root;
            _material = material;
        }

        /// <summary>Sync army views with simulation generals.</summary>
        public void Sync(ArmySystem army, int playerCount)
        {
            if (_root == null || army == null) return;

            // Track which generals exist this frame
            var existingIds = new HashSet<int>();

            for (int p = 0; p < playerCount; p++)
            {
                var generals = army.GetGenerals(p);
                for (int g = 0; g < generals.Count; g++)
                {
                    var gen = generals[g];
                    existingIds.Add(gen.Id);

                    if (!_views.ContainsKey(gen.Id))
                    {
                        var view = ArmyView.Create(_root, gen, _material);
                        _views[gen.Id] = view;
                    }

                    _views[gen.Id].UpdateFromGeneral(gen);
                }
            }

            // Remove views for disbanded generals
            _toRemove.Clear();
            foreach (var kvp in _views)
            {
                if (!existingIds.Contains(kvp.Key))
                    _toRemove.Add(kvp.Key);
            }
            for (int i = 0; i < _toRemove.Count; i++)
            {
                if (_views.TryGetValue(_toRemove[i], out var view))
                {
                    if (view != null) Destroy(view.gameObject);
                    _views.Remove(_toRemove[i]);
                }
            }
        }
    }
}
