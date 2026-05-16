using NUnit.Framework;
using System.Collections.Generic;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for ActionRecord, ActionRecorder, and ReplaySerializer.</summary>
    [TestFixture]
    public class ReplayTests
    {
        // --- ActionRecord ---

        [Test]
        public void ActionRecord_StoresFields()
        {
            var r = new ActionRecord(1.5f, 2, ActionRecord.MOVE_ARMY, "sector=3");
            Assert.AreEqual(1.5f,                r.Timestamp, 0.001f);
            Assert.AreEqual(2,                   r.PlayerId);
            Assert.AreEqual(ActionRecord.MOVE_ARMY, r.ActionType);
            Assert.AreEqual("sector=3",          r.Payload);
        }

        [Test]
        public void ActionRecord_NullPayload_DefaultsToEmpty()
        {
            var r = new ActionRecord(0f, 0, "Foo", null);
            Assert.AreEqual("", r.Payload);
        }

        [Test]
        public void ActionRecord_ToString_ContainsType()
        {
            var r = new ActionRecord(2f, 1, ActionRecord.PLACE_BUILDING, "sector=1");
            StringAssert.Contains(ActionRecord.PLACE_BUILDING, r.ToString());
        }

        // --- ActionRecorder ---

        private static EventBus MakeEventBus() => new EventBus();

        [Test]
        public void ActionRecorder_LogStartsEmpty()
        {
            var rec = new ActionRecorder(MakeEventBus());
            Assert.AreEqual(0, rec.Log.Count);
        }

        [Test]
        public void ActionRecorder_TickAdvancesTime()
        {
            var bus = MakeEventBus();
            var rec = new ActionRecorder(bus);
            rec.Tick(1.5f);

            bus.Publish(new BuildingPlacedEvent(0, 0, BaseBuildingType.Lodge));
            Assert.Greater(rec.Log.Count, 0);
            Assert.AreEqual(1.5f, rec.Log[0].Timestamp, 0.001f);
        }

        [Test]
        public void ActionRecorder_TakeSnapshot_ClearsLog()
        {
            var bus = MakeEventBus();
            var rec = new ActionRecorder(bus);
            bus.Publish(new BuildingPlacedEvent(0, 0, BaseBuildingType.Lodge));
            Assert.Greater(rec.Log.Count, 0);

            var snap = rec.TakeSnapshot();
            Assert.Greater(snap.Count, 0);
            Assert.AreEqual(0, rec.Log.Count, "Log must be cleared after snapshot");
        }

        [Test]
        public void ActionRecorder_SectorConquered_IsRecorded()
        {
            var bus = MakeEventBus();
            var rec = new ActionRecorder(bus);
            bus.Publish(new SectorConqueredEvent(3, 1, Sector.NEUTRAL, ConquestMethod.Military));
            Assert.AreEqual(1, rec.Log.Count);
            Assert.AreEqual(ActionRecord.CONQUER_SECTOR, rec.Log[0].ActionType);
        }

        // --- ReplaySerializer ---

        [Test]
        public void Serialize_ThenDeserialize_PreservesRecords()
        {
            var records = new List<ActionRecord>
            {
                new ActionRecord(0.5f, 0, ActionRecord.PLACE_BUILDING, "sector=1;type=Lodge"),
                new ActionRecord(2.0f, 1, ActionRecord.MOVE_ARMY,       "general=0;target=2"),
            };

            string data    = ReplaySerializer.Serialize(records);
            var    loaded  = ReplaySerializer.Deserialize(data);

            Assert.AreEqual(records.Count, loaded.Count);
            Assert.AreEqual(records[0].Timestamp,  loaded[0].Timestamp,  0.001f);
            Assert.AreEqual(records[0].ActionType, loaded[0].ActionType);
            Assert.AreEqual(records[0].Payload,    loaded[0].Payload);
            Assert.AreEqual(records[1].PlayerId,   loaded[1].PlayerId);
        }

        [Test]
        public void Deserialize_EmptyString_ReturnsEmptyList()
        {
            var result = ReplaySerializer.Deserialize("");
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Deserialize_MalformedLine_IsSkipped()
        {
            string data = "0.0|0|ValidType|payload\nbadline\n0.5|1|OtherType|x";
            var records = ReplaySerializer.Deserialize(data);
            Assert.AreEqual(2, records.Count);
        }

        [Test]
        public void SaveLatest_ThenLoadLatest_RoundTrips()
        {
            var records = new List<ActionRecord>
            {
                new ActionRecord(1.23f, 0, ActionRecord.RESEARCH_TECH, "tech=tier1_lodge"),
            };
            ReplaySerializer.SaveLatest(records);
            var loaded = ReplaySerializer.LoadLatest();

            Assert.Greater(loaded.Count, 0);
            Assert.AreEqual(ActionRecord.RESEARCH_TECH, loaded[0].ActionType);
        }
    }
}
