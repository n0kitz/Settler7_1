using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Simple publish/subscribe event bus for simulation-internal communication.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        /// <summary>Subscribe to events of type T.</summary>
        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }

        /// <summary>Unsubscribe from events of type T.</summary>
        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        /// <summary>Publish an event to all subscribers of type T.</summary>
        public void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
                return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                ((Action<T>)list[i])?.Invoke(evt);
            }
        }

        /// <summary>Remove all subscribers.</summary>
        public void Clear()
        {
            _handlers.Clear();
        }
    }

    // --- Simulation Events ---

    /// <summary>Fired when a building is placed (enters Planned state).</summary>
    public readonly struct BuildingPlacedEvent
    {
        public readonly int BuildingId;
        public readonly int SectorId;
        public readonly BaseBuildingType BuildingType;

        public BuildingPlacedEvent(int buildingId, int sectorId, BaseBuildingType buildingType)
        {
            BuildingId = buildingId;
            SectorId = sectorId;
            BuildingType = buildingType;
        }
    }

    /// <summary>Fired when construction completes on a building.</summary>
    public readonly struct BuildingCompletedEvent
    {
        public readonly int BuildingId;
        public readonly int SectorId;

        public BuildingCompletedEvent(int buildingId, int sectorId)
        {
            BuildingId = buildingId;
            SectorId = sectorId;
        }
    }

    /// <summary>Fired when a work yard is attached to a building.</summary>
    public readonly struct WorkYardAttachedEvent
    {
        public readonly int WorkYardId;
        public readonly int BuildingId;
        public readonly string WorkYardTypeId;

        public WorkYardAttachedEvent(int workYardId, int buildingId, string workYardTypeId)
        {
            WorkYardId = workYardId;
            BuildingId = buildingId;
            WorkYardTypeId = workYardTypeId;
        }
    }

    /// <summary>Fired when a building upgrade completes.</summary>
    public readonly struct BuildingUpgradedEvent
    {
        public readonly int BuildingId;
        public readonly int SectorId;
        public readonly int NewLevel;

        public BuildingUpgradedEvent(int buildingId, int sectorId, int newLevel)
        {
            BuildingId = buildingId;
            SectorId = sectorId;
            NewLevel = newLevel;
        }
    }

    /// <summary>Fired when a building is destroyed (e.g., on sector conquest).</summary>
    public readonly struct BuildingDestroyedEvent
    {
        public readonly int BuildingId;
        public readonly int SectorId;

        public BuildingDestroyedEvent(int buildingId, int sectorId)
        {
            BuildingId = buildingId;
            SectorId = sectorId;
        }
    }

    /// <summary>Fired when a sector is conquered by a player.</summary>
    public readonly struct SectorConqueredEvent
    {
        public readonly int SectorId;
        public readonly int NewOwnerId;
        public readonly int PreviousOwnerId;
        public readonly ConquestMethod Method;

        public SectorConqueredEvent(int sectorId, int newOwner, int prevOwner, ConquestMethod method)
        {
            SectorId = sectorId;
            NewOwnerId = newOwner;
            PreviousOwnerId = prevOwner;
            Method = method;
        }
    }

    /// <summary>Fired when a resource amount changes for a player.</summary>
    public readonly struct ResourceChangedEvent
    {
        public readonly int PlayerId;
        public readonly ResourceType Type;
        public readonly int NewAmount;

        public ResourceChangedEvent(int playerId, ResourceType type, int newAmount)
        {
            PlayerId = playerId;
            Type = type;
            NewAmount = newAmount;
        }
    }
}
