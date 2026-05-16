using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Validates a MapEditorState before allowing playtest or save.
    /// Returns a list of human-readable error strings (empty = valid).
    /// </summary>
    public static class MapValidation
    {
        public static List<string> Validate(MapEditorState state)
        {
            var errors = new List<string>();

            if (state.Sectors.Count < 2)
            {
                errors.Add("Map must have at least 2 sectors.");
                return errors; // graph checks below would throw
            }

            if (!IsConnected(state))
                errors.Add("Map graph is not fully connected — all sectors must be reachable.");

            var playerStarts = new HashSet<int>();
            foreach (var s in state.Sectors)
                if (s.OwnerId >= 0) playerStarts.Add(s.OwnerId);

            if (playerStarts.Count < 1)
                errors.Add("Map must have at least one starting sector (OwnerId >= 0).");

            if (playerStarts.Count > state.MaxPlayers)
                errors.Add($"Map has {playerStarts.Count} starting sectors but MaxPlayers is {state.MaxPlayers}.");

            foreach (var s in state.Sectors)
                if (s.BuildSlots < 1)
                    errors.Add($"Sector '{s.Name}' has 0 build slots.");

            if (state.DefaultVP < 1)
                errors.Add("DefaultVP must be at least 1.");

            if (string.IsNullOrWhiteSpace(state.MapName))
                errors.Add("Map name cannot be empty.");

            return errors;
        }

        private static bool IsConnected(MapEditorState state)
        {
            if (state.Sectors.Count == 0) return true;

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            int start = state.Sectors[0].Id;
            queue.Enqueue(start);
            visited.Add(start);

            // Build adjacency from edges
            var adj = new Dictionary<int, List<int>>();
            foreach (var s in state.Sectors) adj[s.Id] = new List<int>();
            foreach (var (a, b) in state.Edges)
            {
                adj[a].Add(b);
                adj[b].Add(a);
            }

            while (queue.Count > 0)
            {
                int cur = queue.Dequeue();
                if (!adj.ContainsKey(cur)) continue;
                foreach (int nb in adj[cur])
                    if (visited.Add(nb)) queue.Enqueue(nb);
            }

            return visited.Count == state.Sectors.Count;
        }
    }
}
