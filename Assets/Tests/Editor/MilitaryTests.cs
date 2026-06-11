using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class MilitaryTests
    {
        private EventBus _events;
        private PrestigeSystem _prestige;
        private Dictionary<int, PlayerResources> _resources;
        private SectorGraph _graph;
        private ArmySystem _army;
        private CombatResolver _combat;

        [SetUp]
        public void Setup()
        {
            ArmySystem.ResetIdCounter();
            _events = new EventBus();
            _prestige = new PrestigeSystem(5, _events);
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events),
                [1] = new PlayerResources(1, _events)
            };
            _resources[0].Set(ResourceType.Weapons, 50);
            _resources[0].Set(ResourceType.Horses, 10);
            _resources[0].Set(ResourceType.IronBars, 20);
            _resources[0].Set(ResourceType.Cloth, 10);
            _graph = TestMapFactory.CreateSixSectorMap();
            _army = new ArmySystem(_prestige, _resources, _graph, _events);
            _combat = new CombatResolver(_graph, _events);

            // Unlock stronghold + all units for player 0
            _prestige.AwardPoints(0, 30);
            _prestige.TryUnlock(0, "mil_stronghold");
            _prestige.TryUnlock(0, "mil_pikeman");
            _prestige.TryUnlock(0, "mil_musketeer");
            _prestige.TryUnlock(0, "mil_cavalier");
            _prestige.TryUnlock(0, "mil_cannon");
            _prestige.TryUnlock(0, "mil_standard_bearer");
        }

        // --- Generals ---

        [Test]
        public void HireGeneral_Succeeds()
        {
            var gen = _army.HireGeneral(0, 0);
            Assert.IsNotNull(gen);
            Assert.AreEqual(0, gen.OwnerId);
            Assert.AreEqual(0, gen.SectorId);
        }

        [Test]
        public void HireGeneral_FiresEvent()
        {
            int firedId = -1;
            _events.Subscribe<GeneralHiredEvent>(e => firedId = e.GeneralId);
            _army.HireGeneral(0, 0);
            Assert.AreEqual(0, firedId);
        }

        [Test]
        public void SecondGeneral_RequiresPrestigeUnlock()
        {
            _army.HireGeneral(0, 0); // First general
            var gen2 = _army.HireGeneral(0, 0); // Second without unlock
            Assert.IsNull(gen2);

            // SetUp spent all 6 levels on unit unlocks — fund 2 more levels
            _prestige.AwardPoints(0, 10);
            _prestige.TryUnlock(0, "mil_fortification"); // Need this to unlock second_general
            _prestige.TryUnlock(0, "mil_second_general");
            gen2 = _army.HireGeneral(0, 0);
            Assert.IsNotNull(gen2);
        }

        // --- Training ---

        [Test]
        public void TrainUnit_Succeeds()
        {
            Assert.IsTrue(_army.TrainUnit(0, 0, UnitType.Pikeman));
            Assert.AreEqual(1, _army.TrainingQueue.Count);
        }

        [Test]
        public void TrainUnit_SpendsResources()
        {
            int before = _resources[0].Get(ResourceType.Weapons);
            _army.TrainUnit(0, 0, UnitType.Pikeman);
            Assert.AreEqual(before - 1, _resources[0].Get(ResourceType.Weapons));
        }

        [Test]
        public void TrainUnit_CompletesAfterTicking()
        {
            UnitType? trained = null;
            _events.Subscribe<UnitTrainedEvent>(e => trained = e.UnitType);
            _army.TrainUnit(0, 0, UnitType.Pikeman);
            for (int i = 0; i < 200; i++) _army.Tick(0.1f);
            Assert.AreEqual(UnitType.Pikeman, trained);
        }

        [Test]
        public void TrainUnit_FailsWithoutPrestigeUnlock()
        {
            // Player 1 has no unlocks
            _resources[1].Set(ResourceType.Weapons, 10);
            Assert.IsFalse(_army.TrainUnit(1, 1, UnitType.Pikeman));
        }

        // --- Army Movement ---

        [Test]
        public void MoveArmy_StartsMovement()
        {
            var gen = _army.HireGeneral(0, 0);
            gen.AddUnit(UnitType.Pikeman);
            Assert.IsTrue(_army.MoveArmy(gen, 1));
            Assert.IsTrue(gen.IsMoving);
        }

        [Test]
        public void MoveArmy_ArrivesAfterTicking()
        {
            var gen = _army.HireGeneral(0, 0);
            gen.AddUnit(UnitType.Pikeman);
            _army.MoveArmy(gen, 1);

            for (int i = 0; i < 200; i++) _army.Tick(0.1f);
            Assert.AreEqual(1, gen.SectorId);
            Assert.IsFalse(gen.IsMoving);
        }

        // --- Combat ---

        [Test]
        public void Combat_VictoryAgainstWeakGarrison()
        {
            var gen = _army.HireGeneral(0, 0);
            // Large army vs garrison strength 4
            for (int i = 0; i < 20; i++) gen.AddUnit(UnitType.Pikeman);

            var result = _combat.ResolveCombat(gen, 2); // Riverside Meadows, garrison 4
            Assert.IsTrue(result.Victory);
            Assert.AreEqual(0, _graph.GetSector(2).OwnerId);
        }

        [Test]
        public void Combat_FailsAgainstFortifiedWithoutBreachUnits()
        {
            var gen = _army.HireGeneral(0, 0);
            for (int i = 0; i < 10; i++) gen.AddUnit(UnitType.Pikeman); // No breach units

            var result = _combat.ResolveCombat(gen, 3); // Ironpeak Pass, fortified
            Assert.IsFalse(result.Victory);
        }

        [Test]
        public void Combat_SucceedsAgainstFortifiedWithMusketeers()
        {
            var gen = _army.HireGeneral(0, 0);
            for (int i = 0; i < 15; i++) gen.AddUnit(UnitType.Musketeer);
            for (int i = 0; i < 5; i++) gen.AddUnit(UnitType.Cannon);

            var result = _combat.ResolveCombat(gen, 3); // Fortified, garrison 8
            Assert.IsTrue(result.Victory);
        }

        [Test]
        public void Combat_FiresSectorConqueredEvent()
        {
            int conqueredSector = -1;
            _events.Subscribe<SectorConqueredEvent>(e => conqueredSector = e.SectorId);

            var gen = _army.HireGeneral(0, 0);
            for (int i = 0; i < 20; i++) gen.AddUnit(UnitType.Pikeman);
            _combat.ResolveCombat(gen, 2);
            Assert.AreEqual(2, conqueredSector);
        }

        [Test]
        public void StandardBearer_ProvidesMoraleBonus()
        {
            var gen = _army.HireGeneral(0, 0);
            for (int i = 0; i < 5; i++) gen.AddUnit(UnitType.Pikeman);
            int baseAttack = gen.TotalAttack;

            gen.AddUnit(UnitType.StandardBearer);
            int boostedAttack = gen.TotalAttack;

            // StandardBearer adds 15% bonus
            Assert.Greater(boostedAttack, baseAttack);
        }

        // --- Unit Stats ---

        [Test]
        public void UnitStats_AllTypesHavePositiveStats()
        {
            var types = new[] { UnitType.Pikeman, UnitType.Musketeer,
                UnitType.Cavalier, UnitType.Cannon, UnitType.StandardBearer };
            foreach (var t in types)
            {
                Assert.Greater(UnitStats.GetAttack(t), 0);
                Assert.Greater(UnitStats.GetDefense(t), 0);
                Assert.Greater(UnitStats.GetHealth(t), 0);
            }
        }

        [Test]
        public void TotalArmySize_IsCorrect()
        {
            var gen = _army.HireGeneral(0, 0);
            gen.AddUnit(UnitType.Pikeman);
            gen.AddUnit(UnitType.Pikeman);
            gen.AddUnit(UnitType.Musketeer);
            Assert.AreEqual(3, _army.GetTotalArmySize(0));
        }
    }
}
