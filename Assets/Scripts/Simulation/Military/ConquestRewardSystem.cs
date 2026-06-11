using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Conquest reward choice (§14.3, Critical Rule #10): after conquering a
    /// NEUTRAL sector the player picks exactly one of four packages
    /// (1 population reward + 3 conquest reward variants). Never auto-granted
    /// for the human player; AI players choose immediately.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class ConquestRewardSystem
    {
        private readonly Dictionary<int, PlayerResources> _resources;
        private readonly EventBus _events;
        private readonly List<PendingConquestReward> _pending = new();

        public ConquestRewardSystem(Dictionary<int, PlayerResources> resources,
            EventBus events)
        {
            _resources = resources;
            _events = events;
            events.Subscribe<SectorConqueredEvent>(OnConquered);
        }

        /// <summary>All unresolved reward choices.</summary>
        public IReadOnlyList<PendingConquestReward> Pending => _pending;

        /// <summary>First unresolved reward for a player, or null.</summary>
        public PendingConquestReward GetPendingFor(int playerId)
        {
            for (int i = 0; i < _pending.Count; i++)
                if (_pending[i].PlayerId == playerId) return _pending[i];
            return null;
        }

        /// <summary>
        /// Apply one package and resolve the pending reward.
        /// Returns false if there is no matching pending reward or the
        /// package index is invalid.
        /// </summary>
        public bool ChooseReward(int playerId, int sectorId, int packageIndex)
        {
            PendingConquestReward pending = null;
            for (int i = 0; i < _pending.Count; i++)
            {
                if (_pending[i].PlayerId == playerId && _pending[i].SectorId == sectorId)
                {
                    pending = _pending[i];
                    break;
                }
            }
            if (pending == null) return false;
            if (packageIndex < 0 || packageIndex >= pending.Packages.Length) return false;

            if (_resources.TryGetValue(playerId, out var res))
            {
                foreach (var (type, amount) in pending.Packages[packageIndex].Goods)
                    res.Add(type, amount);
            }

            _pending.Remove(pending);
            _events.Publish(new ConquestRewardChosenEvent(playerId, sectorId, packageIndex));
            return true;
        }

        /// <summary>Re-create an unresolved reward from save data.</summary>
        public void RestorePending(int playerId, int sectorId)
        {
            if (GetPendingForSector(playerId, sectorId) != null) return;
            _pending.Add(new PendingConquestReward(playerId, sectorId, CreatePackages()));
        }

        private PendingConquestReward GetPendingForSector(int playerId, int sectorId)
        {
            for (int i = 0; i < _pending.Count; i++)
                if (_pending[i].PlayerId == playerId && _pending[i].SectorId == sectorId)
                    return _pending[i];
            return null;
        }

        private void OnConquered(SectorConqueredEvent evt)
        {
            // Only NEUTRAL conquests grant a reward choice (§14.3)
            if (evt.PreviousOwnerId != Sector.NEUTRAL) return;

            _pending.Add(new PendingConquestReward(
                evt.NewOwnerId, evt.SectorId, CreatePackages()));
            _events.Publish(new ConquestRewardPendingEvent(evt.NewOwnerId, evt.SectorId));

            // AI players pick immediately; the human chooses via the modal
            if (evt.NewOwnerId != 0)
                ChooseReward(evt.NewOwnerId, evt.SectorId, 1);
        }

        /// <summary>§14.3 layout: 1 population package + 3 conquest variants.</summary>
        private static ConquestRewardPackage[] CreatePackages() => new[]
        {
            new ConquestRewardPackage("ui.reward.population",
                new[] { (ResourceType.Bread, 10), (ResourceType.Tools, 5) }),
            new ConquestRewardPackage("ui.reward.conquest",
                new[] { (ResourceType.Planks, 10), (ResourceType.Stone, 5) }),
            new ConquestRewardPackage("ui.reward.conquest",
                new[] { (ResourceType.Weapons, 4), (ResourceType.Horses, 2) }),
            new ConquestRewardPackage("ui.reward.conquest",
                new[] { (ResourceType.Coins, 8), (ResourceType.Jewelry, 2) }),
        };
    }

    /// <summary>One selectable reward package.</summary>
    public class ConquestRewardPackage
    {
        public readonly string TitleKey;
        public readonly (ResourceType type, int amount)[] Goods;

        public ConquestRewardPackage(string titleKey,
            (ResourceType type, int amount)[] goods)
        {
            TitleKey = titleKey;
            Goods = goods;
        }
    }

    /// <summary>An unresolved reward choice after a neutral conquest.</summary>
    public class PendingConquestReward
    {
        public readonly int PlayerId;
        public readonly int SectorId;
        public readonly ConquestRewardPackage[] Packages;

        public PendingConquestReward(int playerId, int sectorId,
            ConquestRewardPackage[] packages)
        {
            PlayerId = playerId;
            SectorId = sectorId;
            Packages = packages;
        }
    }

    /// <summary>Fired when a neutral conquest leaves a reward to choose.</summary>
    public readonly struct ConquestRewardPendingEvent
    {
        public readonly int PlayerId;
        public readonly int SectorId;

        public ConquestRewardPendingEvent(int playerId, int sectorId)
        {
            PlayerId = playerId;
            SectorId = sectorId;
        }
    }

    /// <summary>Fired when a reward package has been chosen and applied.</summary>
    public readonly struct ConquestRewardChosenEvent
    {
        public readonly int PlayerId;
        public readonly int SectorId;
        public readonly int PackageIndex;

        public ConquestRewardChosenEvent(int playerId, int sectorId, int packageIndex)
        {
            PlayerId = playerId;
            SectorId = sectorId;
            PackageIndex = packageIndex;
        }
    }
}
