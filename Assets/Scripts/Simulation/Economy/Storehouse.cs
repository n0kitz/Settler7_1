using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Represents a storehouse in a sector. All goods flow through storehouses.
    /// Each sector needs a storehouse for production to work.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class Storehouse
    {
        private static int _nextId;

        /// <summary>Unique storehouse instance ID.</summary>
        public int Id { get; }

        /// <summary>Sector this storehouse is in.</summary>
        public int SectorId { get; }

        /// <summary>Player who owns this storehouse.</summary>
        public int OwnerId { get; }

        /// <summary>Upgrade level (1-3). Higher = more carriers.</summary>
        public int Level { get; private set; }

        /// <summary>Number of carriers this storehouse has.</summary>
        public int CarrierCount => Level + 1;

        /// <summary>Number of carriers currently idle (not on a delivery).</summary>
        public int IdleCarriers { get; private set; }

        public Storehouse(int sectorId, int ownerId)
        {
            Id = _nextId++;
            SectorId = sectorId;
            OwnerId = ownerId;
            Level = 1;
            IdleCarriers = CarrierCount;
        }

        /// <summary>Dispatch a carrier (returns true if one was available).</summary>
        public bool DispatchCarrier()
        {
            if (IdleCarriers <= 0) return false;
            IdleCarriers--;
            return true;
        }

        /// <summary>Return a carrier after delivery.</summary>
        public void ReturnCarrier()
        {
            IdleCarriers++;
            if (IdleCarriers > CarrierCount)
                IdleCarriers = CarrierCount;
        }

        /// <summary>Upgrade to next level (more carriers).</summary>
        public bool Upgrade()
        {
            if (Level >= 3) return false;
            Level++;
            IdleCarriers++; // New carrier starts idle
            return true;
        }

        /// <summary>Reset ID counter (for tests).</summary>
        public static void ResetIdCounter() => _nextId = 0;
    }
}
