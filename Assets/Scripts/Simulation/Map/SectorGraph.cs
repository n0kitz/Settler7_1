using System;
using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Directed graph of sector adjacency and ownership queries.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class SectorGraph
    {
        private readonly List<Sector> _sectors;
        private readonly Dictionary<int, List<int>> _adjacency;

        public int SectorCount => _sectors.Count;

        public SectorGraph()
        {
            _sectors = new List<Sector>();
            _adjacency = new Dictionary<int, List<int>>();
        }

        /// <summary>Add a sector to the graph. Sector.Id must match insertion order.</summary>
        public void AddSector(Sector sector)
        {
            if (sector.Id != _sectors.Count)
                throw new ArgumentException(
                    $"Sector ID {sector.Id} does not match expected index {_sectors.Count}.");

            _sectors.Add(sector);
            _adjacency[sector.Id] = new List<int>();
        }

        /// <summary>Add a bidirectional edge between two sectors.</summary>
        public void AddEdge(int sectorA, int sectorB)
        {
            ValidateId(sectorA);
            ValidateId(sectorB);

            if (!_adjacency[sectorA].Contains(sectorB))
                _adjacency[sectorA].Add(sectorB);

            if (!_adjacency[sectorB].Contains(sectorA))
                _adjacency[sectorB].Add(sectorA);
        }

        /// <summary>Get a sector by its ID.</summary>
        public Sector GetSector(int id)
        {
            ValidateId(id);
            return _sectors[id];
        }

        /// <summary>Get all sectors in the graph.</summary>
        public IReadOnlyList<Sector> GetAllSectors() => _sectors;

        /// <summary>Get IDs of all sectors adjacent to the given sector.</summary>
        public IReadOnlyList<int> GetNeighbors(int sectorId)
        {
            ValidateId(sectorId);
            return _adjacency[sectorId];
        }

        /// <summary>Check if two sectors are directly adjacent.</summary>
        public bool AreAdjacent(int sectorA, int sectorB)
        {
            ValidateId(sectorA);
            ValidateId(sectorB);
            return _adjacency[sectorA].Contains(sectorB);
        }

        /// <summary>Get all sector IDs owned by a specific player.</summary>
        public List<int> GetSectorsOwnedBy(int playerId)
        {
            var result = new List<int>();
            for (int i = 0; i < _sectors.Count; i++)
            {
                if (_sectors[i].OwnerId == playerId)
                    result.Add(i);
            }
            return result;
        }

        /// <summary>
        /// Check if a player can reach a target sector from any owned sector
        /// by traversing only owned or neutral sectors (for clerics/traders).
        /// Uses BFS.
        /// </summary>
        public bool CanReach(int playerId, int targetSectorId)
        {
            ValidateId(targetSectorId);

            var owned = GetSectorsOwnedBy(playerId);
            if (owned.Count == 0)
                return false;

            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            foreach (int start in owned)
            {
                visited.Add(start);
                queue.Enqueue(start);
            }

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                if (current == targetSectorId)
                    return true;

                foreach (int neighbor in _adjacency[current])
                {
                    if (visited.Contains(neighbor))
                        continue;

                    var sector = _sectors[neighbor];
                    // Can traverse own sectors, neutral sectors, and the target itself
                    if (sector.OwnerId == playerId || sector.IsNeutral ||
                        sector.IsUnowned || neighbor == targetSectorId)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find shortest path (sector IDs) between two sectors using BFS.
        /// Returns empty list if no path exists.
        /// </summary>
        public List<int> FindPath(int fromSectorId, int toSectorId)
        {
            ValidateId(fromSectorId);
            ValidateId(toSectorId);

            if (fromSectorId == toSectorId)
                return new List<int> { fromSectorId };

            var visited = new HashSet<int> { fromSectorId };
            var queue = new Queue<int>();
            var cameFrom = new Dictionary<int, int>();

            queue.Enqueue(fromSectorId);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();

                foreach (int neighbor in _adjacency[current])
                {
                    if (visited.Contains(neighbor))
                        continue;

                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;

                    if (neighbor == toSectorId)
                        return ReconstructPath(cameFrom, fromSectorId, toSectorId);

                    queue.Enqueue(neighbor);
                }
            }

            return new List<int>();
        }

        private List<int> ReconstructPath(Dictionary<int, int> cameFrom, int from, int to)
        {
            var path = new List<int>();
            int current = to;
            while (current != from)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(from);
            path.Reverse();
            return path;
        }

        private void ValidateId(int sectorId)
        {
            if (sectorId < 0 || sectorId >= _sectors.Count)
                throw new ArgumentOutOfRangeException(nameof(sectorId),
                    $"Sector ID {sectorId} is out of range [0, {_sectors.Count}).");
        }
    }
}
