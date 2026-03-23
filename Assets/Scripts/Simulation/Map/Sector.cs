using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Simulation-layer data for a single map sector.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class Sector
    {
        public const int UNOWNED = -1;
        public const int NEUTRAL = -2;

        /// <summary>Unique sector index (0-based, matches graph index).</summary>
        public int Id { get; }

        /// <summary>Display name shown in UI (e.g. "Northern Forest").</summary>
        public string Name { get; }

        /// <summary>
        /// Player ID that owns this sector.
        /// UNOWNED (-1) = nobody, NEUTRAL (-2) = has a garrison but no player.
        /// </summary>
        public int OwnerId { get; private set; }

        /// <summary>Garrison strength for neutral sectors. 0 if player-owned.</summary>
        public int GarrisonStrength { get; private set; }

        /// <summary>Whether the sector has fortifications (walls).</summary>
        public bool IsFortified { get; private set; }

        /// <summary>Resource deposits available in this sector.</summary>
        public IReadOnlyList<ResourceNodeType> ResourceNodes => _resourceNodes;

        /// <summary>Maximum number of building slots in this sector.</summary>
        public int BuildSlots { get; }

        /// <summary>If non-null, conquering this sector grants a permanent VP with this ID.</summary>
        public string VPRewardId { get; }

        private readonly List<ResourceNodeType> _resourceNodes;

        public Sector(int id, string name, int ownerId, int garrisonStrength,
            bool isFortified, List<ResourceNodeType> resourceNodes, int buildSlots,
            string vpRewardId = null)
        {
            Id = id;
            Name = name;
            OwnerId = ownerId;
            GarrisonStrength = garrisonStrength;
            IsFortified = isFortified;
            _resourceNodes = resourceNodes ?? new List<ResourceNodeType>();
            BuildSlots = buildSlots;
            VPRewardId = vpRewardId;
        }

        /// <summary>Returns true if no player owns this sector.</summary>
        public bool IsUnowned => OwnerId == UNOWNED;

        /// <summary>Returns true if this sector has a neutral garrison.</summary>
        public bool IsNeutral => OwnerId == NEUTRAL;

        /// <summary>Returns true if a player owns this sector.</summary>
        public bool IsPlayerOwned => OwnerId >= 0;

        /// <summary>
        /// Transfer ownership of this sector to a player.
        /// Clears garrison strength on conquest.
        /// </summary>
        public void SetOwner(int playerId)
        {
            OwnerId = playerId;
            GarrisonStrength = 0;
        }

        /// <summary>Set the garrison strength.</summary>
        public void SetGarrison(int strength)
        {
            GarrisonStrength = strength;
        }

        /// <summary>Set whether this sector is fortified.</summary>
        public void SetFortified(bool fortified)
        {
            IsFortified = fortified;
        }

        /// <summary>Check if this sector has a specific resource deposit.</summary>
        public bool HasResource(ResourceNodeType nodeType)
        {
            return _resourceNodes.Contains(nodeType);
        }
    }
}
