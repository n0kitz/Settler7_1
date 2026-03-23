using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class VictoryTests
    {
        private GameState _state;
        private VictorySystem _victory;

        [SetUp]
        public void Setup()
        {
            Building.ResetIdCounter();
            Storehouse.ResetIdCounter();
            ArmySystem.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            _state = new GameState(graph, 2, 10f, 3);
            _victory = _state.Victory;
        }

        // --- VP Counting ---

        [Test]
        public void NewGame_ZeroVPs()
        {
            Assert.AreEqual(0, _victory.GetVPCount(0));
            Assert.AreEqual(0, _victory.GetVPCount(1));
        }

        [Test]
        public void PermanentVP_CanBeAwarded()
        {
            _victory.AwardPermanentVP(0, "vp_abbey");
            Assert.IsTrue(_victory.HasVP(0, "vp_abbey"));
            Assert.AreEqual(1, _victory.GetVPCount(0));
        }

        [Test]
        public void PermanentVP_CannotBeLost()
        {
            _victory.AwardPermanentVP(0, "vp_abbey");
            // Tick shouldn't remove it
            _victory.Tick(1f);
            Assert.IsTrue(_victory.HasVP(0, "vp_abbey"));
        }

        // --- Dynamic VPs ---

        [Test]
        public void Emperor_GainedWith3Sectors()
        {
            // Player 0 starts with sector 0, conquer 2 more
            _state.Graph.GetSector(2).SetOwner(0);
            _state.Graph.GetSector(4).SetOwner(0);
            _victory.Tick(1f);
            Assert.IsTrue(_victory.HasVP(0, "vp_emperor"));
        }

        [Test]
        public void Emperor_LostWhenSectorsTaken()
        {
            _state.Graph.GetSector(2).SetOwner(0);
            _state.Graph.GetSector(4).SetOwner(0);
            _victory.Tick(1f);
            Assert.IsTrue(_victory.HasVP(0, "vp_emperor"));

            // Lose a sector
            _state.Graph.GetSector(4).SetOwner(1);
            _victory.Tick(1f);
            Assert.IsFalse(_victory.HasVP(0, "vp_emperor"));
        }

        [Test]
        public void Banker_GainedWith25Coins()
        {
            _state.PlayerResources[0].Set(ResourceType.Coins, 25);
            _victory.Tick(1f);
            Assert.IsTrue(_victory.HasVP(0, "vp_banker"));
        }

        [Test]
        public void SunKing_GainedWithPrestigeLevel5()
        {
            _state.Prestige.AwardPoints(0, 25); // Level 5
            _victory.Tick(1f);
            Assert.IsTrue(_victory.HasVP(0, "vp_sun_king"));
        }

        [Test]
        public void DynamicVP_StolenByBetterPlayer()
        {
            _state.PlayerResources[0].Set(ResourceType.Coins, 25);
            _victory.Tick(1f);
            Assert.IsTrue(_victory.HasVP(0, "vp_banker"));

            // Player 1 gets more coins
            _state.PlayerResources[1].Set(ResourceType.Coins, 30);
            _state.PlayerResources[0].Set(ResourceType.Coins, 10); // Player 0 loses
            _victory.Tick(1f);
            Assert.IsFalse(_victory.HasVP(0, "vp_banker"));
            Assert.IsTrue(_victory.HasVP(1, "vp_banker"));
        }

        // --- Countdown ---

        [Test]
        public void Countdown_StartsWhenVPReached()
        {
            // Award 4 permanent VPs
            _victory.AwardPermanentVP(0, "vp_1");
            _victory.AwardPermanentVP(0, "vp_2");
            _victory.AwardPermanentVP(0, "vp_3");
            _victory.AwardPermanentVP(0, "vp_4");

            _victory.Tick(1f);
            Assert.IsTrue(_victory.IsCountdownActive);
            Assert.AreEqual(0, _victory.CountdownPlayerId);
        }

        [Test]
        public void Countdown_CancelledIfVPsLost()
        {
            // Use dynamic VPs that can be lost
            _state.PlayerResources[0].Set(ResourceType.Coins, 25);
            _state.Graph.GetSector(2).SetOwner(0);
            _state.Graph.GetSector(4).SetOwner(0);
            _state.Prestige.AwardPoints(0, 25);
            _victory.AwardPermanentVP(0, "vp_test");
            _victory.Tick(1f);

            // Should have 4 VPs: emperor + banker + sun_king + permanent
            Assert.IsTrue(_victory.IsCountdownActive);

            // Lose coins
            _state.PlayerResources[0].Set(ResourceType.Coins, 0);
            _state.Prestige.AwardPoints(1, 0); // No change
            _victory.Tick(1f);
            // Now only 3 VPs — countdown cancelled
            Assert.IsFalse(_victory.IsCountdownActive);
        }

        [Test]
        public void GameOver_AfterCountdownExpires()
        {
            _victory.AwardPermanentVP(0, "vp_1");
            _victory.AwardPermanentVP(0, "vp_2");
            _victory.AwardPermanentVP(0, "vp_3");
            _victory.AwardPermanentVP(0, "vp_4");

            // Tick through entire countdown (180s)
            for (int i = 0; i < 200; i++) _victory.Tick(1f);

            Assert.IsTrue(_victory.IsGameOver);
            Assert.AreEqual(0, _victory.WinnerId);
        }

        [Test]
        public void GameOver_FiresEvent()
        {
            int winner = -1;
            _state.Events.Subscribe<GameOverEvent>(e => winner = e.WinnerId);

            _victory.AwardPermanentVP(0, "vp_1");
            _victory.AwardPermanentVP(0, "vp_2");
            _victory.AwardPermanentVP(0, "vp_3");
            _victory.AwardPermanentVP(0, "vp_4");

            for (int i = 0; i < 200; i++) _victory.Tick(1f);
            Assert.AreEqual(0, winner);
        }

        [Test]
        public void VPRequired_IsCorrect()
        {
            Assert.AreEqual(4, _victory.VPRequired);
        }

        // --- VP Events ---

        [Test]
        public void VPChange_FiresEvent()
        {
            bool fired = false;
            _state.Events.Subscribe<VPChangedEvent>(e => fired = true);
            _victory.AwardPermanentVP(0, "vp_test");
            Assert.IsTrue(fired);
        }
    }
}
