using NUnit.Framework;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for MapEditorState, MapValidation, and MapSerializer.</summary>
    [TestFixture]
    public class MapEditorTests
    {
        private MapEditorState MakeValidState()
        {
            var state = new MapEditorState();
            state.MapName = "Test Map";
            state.MaxPlayers = 2;
            state.DefaultVP = 4;
            var s0 = state.AddSector(0f, 0f, ownerId: 0);  // player 0 start
            var s1 = state.AddSector(5f, 0f, ownerId: 1);  // player 1 start
            state.AddEdge(s0.Id, s1.Id);
            return state;
        }

        // --- MapEditorState ---

        [Test]
        public void AddSector_IncreasesCount()
        {
            var state = new MapEditorState();
            state.AddSector(0f, 0f);
            Assert.AreEqual(1, state.Sectors.Count);
        }

        [Test]
        public void RemoveSector_DecreasesCount()
        {
            var state = new MapEditorState();
            var s = state.AddSector(0f, 0f);
            state.RemoveSector(s.Id);
            Assert.AreEqual(0, state.Sectors.Count);
        }

        [Test]
        public void RemoveSector_AlsoRemovesConnectedEdges()
        {
            var state = new MapEditorState();
            var s0 = state.AddSector(0f, 0f);
            var s1 = state.AddSector(5f, 0f);
            state.AddEdge(s0.Id, s1.Id);
            Assert.AreEqual(1, state.Edges.Count);
            state.RemoveSector(s0.Id);
            Assert.AreEqual(0, state.Edges.Count);
        }

        [Test]
        public void AddEdge_DuplicateIsIgnored()
        {
            var state = new MapEditorState();
            var s0 = state.AddSector(0f, 0f);
            var s1 = state.AddSector(5f, 0f);
            state.AddEdge(s0.Id, s1.Id);
            state.AddEdge(s0.Id, s1.Id); // duplicate
            state.AddEdge(s1.Id, s0.Id); // reversed duplicate
            Assert.AreEqual(1, state.Edges.Count);
        }

        [Test]
        public void ToSectorGraph_BuildsCorrectGraph()
        {
            var state = MakeValidState();
            var graph = state.ToSectorGraph();
            Assert.AreEqual(2, graph.GetSectorsOwnedBy(0).Count + graph.GetSectorsOwnedBy(1).Count);
        }

        [Test]
        public void Clear_ResetsAllData()
        {
            var state = MakeValidState();
            state.Clear();
            Assert.AreEqual(0, state.Sectors.Count);
            Assert.AreEqual(0, state.Edges.Count);
        }

        // --- MapValidation ---

        [Test]
        public void Validate_ValidMap_ReturnsNoErrors()
        {
            var state = MakeValidState();
            var errors = MapValidation.Validate(state);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void Validate_SingleSector_ReturnsError()
        {
            var state = new MapEditorState();
            state.MapName = "X";
            state.AddSector(0f, 0f, 0);
            var errors = MapValidation.Validate(state);
            Assert.Greater(errors.Count, 0);
        }

        [Test]
        public void Validate_DisconnectedGraph_ReturnsError()
        {
            var state = new MapEditorState();
            state.MapName = "X";
            state.MaxPlayers = 2;
            state.AddSector(0f, 0f, 0);  // no edge to second
            state.AddSector(5f, 0f, 1);
            // No edge — disconnected
            var errors = MapValidation.Validate(state);
            Assert.IsTrue(errors.Exists(e => e.Contains("connected")));
        }

        [Test]
        public void Validate_NoStartingSectors_ReturnsError()
        {
            var state = new MapEditorState();
            state.MapName = "X";
            state.MaxPlayers = 2;
            var s0 = state.AddSector(0f, 0f, Sector.NEUTRAL);
            var s1 = state.AddSector(5f, 0f, Sector.NEUTRAL);
            state.AddEdge(s0.Id, s1.Id);
            var errors = MapValidation.Validate(state);
            Assert.IsTrue(errors.Exists(e => e.Contains("starting")));
        }

        [Test]
        public void Validate_EmptyMapName_ReturnsError()
        {
            var state = MakeValidState();
            state.MapName = "";
            var errors = MapValidation.Validate(state);
            Assert.IsTrue(errors.Exists(e => e.Contains("name")));
        }

        // --- MapSerializer ---

        [Test]
        public void Serialize_ThenDeserialize_PreservesMapName()
        {
            var state = MakeValidState();
            state.MapName = "Round Trip Map";
            string json = MapSerializer.Serialize(state);
            var loaded = MapSerializer.Deserialize(json, out string error);
            Assert.IsNull(error);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Round Trip Map", loaded.MapName);
        }

        [Test]
        public void Serialize_ThenDeserialize_PreservesSectorCount()
        {
            var state = MakeValidState();
            string json = MapSerializer.Serialize(state);
            var loaded = MapSerializer.Deserialize(json, out string error);
            Assert.IsNull(error);
            Assert.AreEqual(state.Sectors.Count, loaded.Sectors.Count);
        }

        [Test]
        public void Serialize_ThenDeserialize_PreservesEdgeCount()
        {
            var state = MakeValidState();
            string json = MapSerializer.Serialize(state);
            var loaded = MapSerializer.Deserialize(json, out string error);
            Assert.IsNull(error);
            Assert.AreEqual(state.Edges.Count, loaded.Edges.Count);
        }

        [Test]
        public void Deserialize_EmptyString_ReturnsNullWithError()
        {
            var result = MapSerializer.Deserialize("", out string error);
            Assert.IsNull(result);
            Assert.IsNotNull(error);
        }
    }
}
