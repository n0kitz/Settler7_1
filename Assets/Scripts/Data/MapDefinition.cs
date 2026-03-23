using System;
using UnityEngine;

namespace Settlers.Data
{
    /// <summary>
    /// ScriptableObject defining a complete map layout.
    /// Create via Assets > Create > Settlers > MapDefinition.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMap", menuName = "Settlers/MapDefinition")]
    public class MapDefinition : ScriptableObject
    {
        public string mapId;
        public string displayName;

        [Tooltip("Number of supported players (1-4).")]
        [Range(1, 4)]
        public int playerCount = 2;

        [Tooltip("VPs required to trigger the victory countdown.")]
        public int victoryPointsRequired = 4;

        public SectorDefinition[] sectors;
        public EdgeDefinition[] edges;
        public PlayerStartDefinition[] playerStarts;
    }

    /// <summary>
    /// Defines a single sector within a MapDefinition.
    /// </summary>
    [Serializable]
    public class SectorDefinition
    {
        public string sectorName;

        [Tooltip("World-space center position of this sector.")]
        public Vector3 position;

        [Tooltip("-1 = unowned, -2 = neutral with garrison, 0+ = player ID.")]
        public int initialOwner = -1;

        public int garrisonStrength;
        public bool isFortified;
        public int buildSlots = 6;

        public Settlers.Simulation.ResourceNodeType[] resourceNodes;
    }

    /// <summary>
    /// Defines a bidirectional edge between two sectors by index.
    /// </summary>
    [Serializable]
    public struct EdgeDefinition
    {
        public int sectorA;
        public int sectorB;
    }

    /// <summary>
    /// Defines where each player begins on the map.
    /// </summary>
    [Serializable]
    public struct PlayerStartDefinition
    {
        [Tooltip("Index into the sectors array.")]
        public int sectorIndex;

        [Tooltip("Player ID (0-based).")]
        public int playerId;
    }
}
