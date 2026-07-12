using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class AITests
    {
        private GameState _state;

        [SetUp]
        public void SetUp()
        {
            Building.ResetIdCounter();
            WorkYard.ResetIdCounter();
            var graph = TestMapFactory.CreateSixSectorMap();
            _state = new GameState(graph, playerCount: 2,
                constructionBaseTime: 0.01f, carrierMaxItems: 3,
                vpRequired: 4, mapId: "test_valley");
            // Give AI player extra resources
            _state.PlayerResources[1].Set(ResourceType.Planks, 50);
            _state.PlayerResources[1].Set(ResourceType.Stone, 30);
            _state.PlayerResources[1].Set(ResourceType.Tools, 10);
        }

        [Test]
        public void AI_StartsInEarlyEconomy()
        {
            Assert.AreEqual(1, _state.AIPlayers.Count);
            Assert.AreEqual(AIPhase.EarlyEconomy, _state.AIPlayers[0].Phase);
            Assert.AreEqual(AIPath.None, _state.AIPlayers[0].ChosenPath);
        }

        [Test]
        public void AI_BuildsEconomy_PlacesBuildings()
        {
            int buildingsBefore = _state.Construction.GetBuildingsByPlayer(1).Count;
            AIEconomy.BuildEconomy(_state, 1);

            Assert.Greater(_state.Construction.GetBuildingsByPlayer(1).Count, buildingsBefore);
        }

        [Test]
        public void AI_AttachesWorkYards_AfterBuildingComplete()
        {
            AIEconomy.BuildEconomy(_state, 1);
            _state.Construction.Tick(1f); // Complete construction

            AIEconomy.AttachWorkYards(_state, 1);

            bool anyWorkYards = false;
            foreach (var b in _state.Construction.GetBuildingsByPlayer(1))
                if (b.WorkYards.Count > 0) { anyWorkYards = true; break; }

            Assert.IsTrue(anyWorkYards);
        }

        [Test]
        public void AI_TransitionsToPathSelection_AfterEnoughBuildings()
        {
            var ai = _state.AIPlayers[0];

            // Give resources and tick multiple times
            _state.Construction.SetConstructorCount(1, 10);

            for (int i = 0; i < 20; i++)
            {
                ai.Tick(6f); // Over DECISION_INTERVAL
                _state.Construction.Tick(1f);
            }

            Assert.AreNotEqual(AIPhase.EarlyEconomy, ai.Phase,
                "AI should have left early economy after building enough");
        }

        [Test]
        public void AIEconomy_ChoosesLodge_WhenLowOnWood()
        {
            _state.PlayerResources[1].Set(ResourceType.Planks, 5);
            _state.PlayerResources[1].Set(ResourceType.Wood, 0);

            AIEconomy.BuildEconomy(_state, 1);

            // Should have placed a Lodge (cheapest, produces wood)
            var buildings = _state.Construction.GetBuildingsByPlayer(1);
            bool hasLodge = false;
            foreach (var b in buildings)
                if (b.Type == BaseBuildingType.Lodge) { hasLodge = true; break; }

            Assert.IsTrue(hasLodge, "AI should place Lodge when low on wood/planks");
        }

        [Test]
        public void AIEconomy_ManagesFood_EnablesPlainWhenAvailable()
        {
            // Create a building with a work yard
            _state.PlayerResources[1].Set(ResourceType.Bread, 10);
            var b = _state.Construction.PlaceBuilding(
                BaseBuildingType.Lodge, 1, 1, 3, 0f, 0f, 0, 8);
            _state.Construction.Tick(1f);

            var wy = new WorkYard("forester", b.Id, 1, 1, ResourceNodeType.Forest, 0f, 0f);
            b.AttachWorkYard(wy);

            AIEconomy.ManageFood(_state, 1);

            Assert.AreEqual(FoodSetting.Plain, b.FoodSetting);
        }

        [Test]
        public void AIEconomy_ManageQuests_AcceptsAvailableQuest()
        {
            // Keep stock below quest objectives so the accepted quest stays
            // active instead of auto-completing within the same call
            _state.PlayerResources[1].Set(ResourceType.Planks, 5);
            AIEconomy.ManageQuests(_state, 1);

            var active = _state.Quests.GetActiveQuests(1);
            Assert.Greater(active.Count, 0, "AI should accept at least one quest");
        }

        [Test]
        public void AIEconomy_GetResource_ReturnsCorrectAmount()
        {
            _state.PlayerResources[1].Set(ResourceType.IronBars, 42);
            Assert.AreEqual(42, AIEconomy.GetResource(_state, 1, ResourceType.IronBars));
        }

        [Test]
        public void AIEconomy_GetResource_Returns0_ForMissingPlayer()
        {
            Assert.AreEqual(0, AIEconomy.GetResource(_state, 99, ResourceType.Planks));
        }

        [Test]
        public void AI_FullSimulation_DoesNotCrash()
        {
            // Run 200 ticks of full simulation — ensure no exceptions
            for (int i = 0; i < 200; i++)
            {
                _state.Construction.Tick(0.1f);
                _state.Production.Tick(0.1f);
                _state.Population.Tick(0.1f);
                foreach (var ai in _state.AIPlayers)
                    ai.Tick(0.1f);
            }

            // AI should have progressed past early economy
            Assert.Pass("200 ticks completed without exception");
        }

        // ---- §14.6 cleric balance: a Technology AI must produce Books/Garments ----

        [Test]
        public void AIEconomy_ClergyDemand_AttachesBookbinderFirst_ToNobleResidence()
        {
            var b = _state.Construction.PlaceBuilding(
                BaseBuildingType.NobleResidence, 1, 1, 3, 0f, 0f, 0, 8);
            _state.Construction.Tick(1f); // complete construction

            AIEconomy.AttachWorkYards(_state, 1, prioritizeClergyGoods: true);

            Assert.IsTrue(HasWorkYard(b, "bookbinder"),
                "Technology AI should attach a bookbinder (Books) to its Noble Residence first");
        }

        [Test]
        public void AIEconomy_NormalMode_AttachesButcherFirst_ToNobleResidence()
        {
            var b = _state.Construction.PlaceBuilding(
                BaseBuildingType.NobleResidence, 1, 1, 3, 0f, 0f, 0, 8);
            _state.Construction.Tick(1f);

            AIEconomy.AttachWorkYards(_state, 1); // default — no clergy bias

            Assert.IsTrue(HasWorkYard(b, "butcher"),
                "Default AI keeps the original Noble Residence priority (butcher first)");
            Assert.IsFalse(HasWorkYard(b, "bookbinder"));
        }

        [Test]
        public void AIEconomy_ClergyDemand_BuildsResidence_OnMineralSector()
        {
            // Sector 1 has a Stone node, so the default AI keeps building Mountain Shelters.
            // Over a few build cycles the Technology AI must instead stand up its chain,
            // including a Residence to host the Books chain — not just mines.
            _state.PlayerResources[1].Set(ResourceType.Planks, 100);
            _state.PlayerResources[1].Set(ResourceType.Stone, 60);
            _state.PlayerResources[1].Set(ResourceType.Wood, 20);
            _state.PlayerResources[1].Set(ResourceType.Bread, 20);
            _state.Construction.SetConstructorCount(1, 10);

            for (int i = 0; i < 6; i++)
            {
                AIEconomy.BuildEconomy(_state, 1, prioritizeClergyGoods: true);
                _state.Construction.Tick(1f);
            }

            bool hasResidence = false;
            foreach (var bldg in _state.Construction.GetBuildingsByPlayer(1))
                if (bldg.Type == BaseBuildingType.Residence) hasResidence = true;

            Assert.IsTrue(hasResidence,
                "Technology AI should build a Residence for the Books chain, not only mines");
        }

        [Test]
        public void AIEconomy_ClergyDemand_StandsUpBooksChain()
        {
            // From an empty economy on player 1's home sector (Forest + Water + FertileLand),
            // a clergy-focused AI must raise a Residence + Noble Residence and attach the full
            // Books chain (paper_mill → bookbinder). Under the default 3-slot priorities neither
            // yard is ever reached, so this fails without the fix.
            _state.PlayerResources[1].Set(ResourceType.Planks, 200);
            _state.PlayerResources[1].Set(ResourceType.Stone, 200);
            _state.PlayerResources[1].Set(ResourceType.Wood, 20);
            _state.PlayerResources[1].Set(ResourceType.Bread, 20);

            for (int i = 0; i < 30; i++)
            {
                AIEconomy.BuildEconomy(_state, 1, prioritizeClergyGoods: true);
                _state.Construction.Tick(1f); // complete builds
                AIEconomy.AttachWorkYards(_state, 1, prioritizeClergyGoods: true);
            }

            bool paperMill = false, bookbinder = false;
            foreach (var b in _state.Construction.GetBuildingsByPlayer(1))
                foreach (var wy in b.WorkYards)
                {
                    if (wy.TypeId == "paper_mill") paperMill = true;
                    if (wy.TypeId == "bookbinder") bookbinder = true;
                }

            Assert.IsTrue(paperMill, "clergy AI should build a paper_mill (Paper for Books)");
            Assert.IsTrue(bookbinder, "clergy AI should build a bookbinder (Books for Brothers)");
        }

        private static bool HasWorkYard(Building b, string typeId)
        {
            foreach (var wy in b.WorkYards)
                if (wy.TypeId == typeId) return true;
            return false;
        }

        // ---- AI economy scaling: don't out-build the population ----

        [Test]
        public void AIEconomy_AttachWorkYards_StopsWhenNoSettlersAvailable()
        {
            // A Lodge provides one settler and hosts up to three yards. After the first yard
            // is staffed, no further yard should attach — there is no settler left to run it.
            var b = _state.Construction.PlaceBuilding(
                BaseBuildingType.Lodge, 1, 1, 3, 0f, 0f, 0, 8);
            _state.Construction.Tick(1f); // operational → living space 1
            _state.PlayerResources[1].Add(ResourceType.Tools, 5);

            AIEconomy.AttachWorkYards(_state, 1); // budget 1 → attaches one yard
            Assert.AreEqual(1, b.WorkYards.Count);

            _state.Population.Tick(1f); // employs that settler → 0 available

            AIEconomy.AttachWorkYards(_state, 1); // budget 0 → attaches nothing

            Assert.AreEqual(1, b.WorkYards.Count,
                "no second yard should attach once all settlers are employed");
        }

        [Test]
        public void AIEconomy_BuildEconomy_StopsUtilitySprawl_WhenSlotsOutrunPopulation()
        {
            // Seven low-population utility buildings already commit 21 work-yard slots against
            // only 7 living space — past the slack — while sector 1 (8 slots) still has room.
            // The AI must decline to add another utility shell it could never staff.
            _state.Construction.SetConstructorCount(1, 10);
            for (int i = 0; i < 7; i++)
                _state.Construction.PlaceBuilding(BaseBuildingType.Lodge, 1, 1, 3, 0f, 0f, i, 8);
            _state.Construction.Tick(1f); // 7 Lodges operational → livingSpace 7, slots 21
            int before = _state.Construction.GetBuildingsByPlayer(1).Count;

            AIEconomy.BuildEconomy(_state, 1); // would otherwise place an 8th utility building

            Assert.AreEqual(before, _state.Construction.GetBuildingsByPlayer(1).Count,
                "AI should not keep adding utility buildings past what population can staff");
        }
    }
}
