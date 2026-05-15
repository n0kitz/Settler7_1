using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Pure-data model for the in-game map editor.
    /// Tracks placed sectors and road edges independently of SectorGraph
    /// so the editor can be undone/redone without touching the live simulation.
    /// </summary>
    public sealed class MapEditorState
    {
        public sealed class EditorSector
        {
            public int Id;
            public string Name;
            public int OwnerId;           // -1 = neutral
            public int GarrisonStrength;
            public bool IsFortified;
            public List<ResourceNodeType> ResourceNodes = new List<ResourceNodeType>();
            public int BuildSlots = 4;
            public float X;               // 2-D layout position
            public float Y;
        }

        private readonly List<EditorSector> _sectors = new List<EditorSector>();
        private readonly List<(int A, int B)> _edges = new List<(int, int)>();
        private int _nextId;

        public string MapName = "My Map";
        public string MapDescription = "";
        public int MaxPlayers = 2;
        public int DefaultVP = 4;

        public IReadOnlyList<EditorSector> Sectors => _sectors;
        public IReadOnlyList<(int A, int B)> Edges => _edges;

        /// <summary>Add a new sector at a given editor position and return it.</summary>
        public EditorSector AddSector(float x, float y, int ownerId = Sector.NEUTRAL)
        {
            var s = new EditorSector
            {
                Id = _nextId++,
                Name = $"Sector {_nextId}",
                OwnerId = ownerId,
                GarrisonStrength = 5,
                X = x,
                Y = y,
            };
            _sectors.Add(s);
            return s;
        }

        /// <summary>Remove a sector and all edges connected to it.</summary>
        public void RemoveSector(int id)
        {
            _sectors.RemoveAll(s => s.Id == id);
            _edges.RemoveAll(e => e.A == id || e.B == id);
        }

        /// <summary>Add a road between two sectors if it doesn't already exist.</summary>
        public bool AddEdge(int a, int b)
        {
            if (a == b) return false;
            foreach (var e in _edges)
                if ((e.A == a && e.B == b) || (e.A == b && e.B == a))
                    return false;
            _edges.Add((a, b));
            return true;
        }

        public void RemoveEdge(int a, int b)
        {
            _edges.RemoveAll(e => (e.A == a && e.B == b) || (e.A == b && e.B == a));
        }

        public EditorSector FindSector(int id)
        {
            foreach (var s in _sectors)
                if (s.Id == id) return s;
            return null;
        }

        /// <summary>
        /// Build a SectorGraph from the current editor state for playtesting.
        /// Remaps editor IDs to sequential 0-based indices as required by SectorGraph.
        /// </summary>
        public SectorGraph ToSectorGraph()
        {
            // Build ID → sequential index map
            var idToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _sectors.Count; i++)
                idToIndex[_sectors[i].Id] = i;

            var graph = new SectorGraph();
            for (int i = 0; i < _sectors.Count; i++)
            {
                var s = _sectors[i];
                var nodes = new List<ResourceNodeType>(s.ResourceNodes);
                graph.AddSector(new Sector(i, s.Name, s.OwnerId,
                    s.GarrisonStrength, s.IsFortified, nodes, s.BuildSlots));
            }

            foreach (var (a, b) in _edges)
            {
                if (idToIndex.TryGetValue(a, out int ia) &&
                    idToIndex.TryGetValue(b, out int ib))
                    graph.AddEdge(ia, ib);
            }

            return graph;
        }

        public void Clear()
        {
            _sectors.Clear();
            _edges.Clear();
            _nextId = 0;
        }
    }
}
