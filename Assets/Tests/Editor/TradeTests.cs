using System.Collections.Generic;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class TradeTests
    {
        private EventBus _events;
        private PrestigeSystem _prestige;
        private Dictionary<int, PlayerResources> _resources;
        private TradeMap _tradeMap;
        private TradeSystem _trade;

        [SetUp]
        public void Setup()
        {
            _events = new EventBus();
            _prestige = new PrestigeSystem(5, _events);
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events),
                [1] = new PlayerResources(1, _events)
            };
            _resources[0].Set(ResourceType.Planks, 50);
            _resources[0].Set(ResourceType.Coins, 50);
            _tradeMap = TestTradeMapFactory.CreateTestTradeMap();
            _trade = new TradeSystem(_tradeMap, _resources, _prestige, _events);

            // Unlock export office for player 0
            _prestige.AwardPoints(0, 5);
            _prestige.TryUnlock(0, "cul_export_office");
        }

        // --- Trade Map ---

        [Test]
        public void TradeMap_Has8Outposts()
        {
            Assert.AreEqual(8, _tradeMap.AllOutposts.Count);
        }

        [Test]
        public void TradeMap_HasSpecialOutposts()
        {
            int special = 0;
            foreach (var op in _tradeMap.AllOutposts)
                if (op.IsSpecial) special++;
            Assert.AreEqual(2, special);
        }

        // --- Sending Traders ---

        [Test]
        public void SendTrader_Succeeds()
        {
            Assert.IsTrue(_trade.SendTrader(0, "trade_planks_iron"));
            Assert.AreEqual(1, _trade.ActiveTasks.Count);
        }

        [Test]
        public void SendTrader_FailsWithoutExportOfficeUnlock()
        {
            Assert.IsFalse(_trade.SendTrader(1, "trade_planks_iron"));
        }

        [Test]
        public void SendTrader_ClaimsOutpostAfterTicking()
        {
            string claimed = null;
            _events.Subscribe<OutpostClaimedEvent>(e => claimed = e.OutpostId);
            _trade.SendTrader(0, "trade_planks_iron");
            for (int i = 0; i < 300; i++) _trade.Tick(0.1f);
            Assert.AreEqual("trade_planks_iron", claimed);
            Assert.AreEqual(0, _tradeMap.GetOutpost("trade_planks_iron").ClaimedBy);
        }

        [Test]
        public void SendTrader_FailsIfAlreadyClaimed()
        {
            _trade.SendTrader(0, "trade_planks_iron");
            for (int i = 0; i < 300; i++) _trade.Tick(0.1f);
            // Already claimed by player 0
            _prestige.AwardPoints(1, 5);
            _prestige.TryUnlock(1, "cul_export_office");
            Assert.IsFalse(_trade.SendTrader(1, "trade_planks_iron"));
        }

        // --- Executing Trades ---

        [Test]
        public void ExecuteTrade_ExchangesResources()
        {
            // Claim outpost first
            var outpost = _tradeMap.GetOutpost("trade_planks_iron");
            outpost.TryClaim(0);

            int planksBefore = _resources[0].Get(ResourceType.Planks);
            int ironBefore = _resources[0].Get(ResourceType.IronBars);

            Assert.IsTrue(_trade.ExecuteTrade(0, "trade_planks_iron"));
            Assert.AreEqual(planksBefore - 3, _resources[0].Get(ResourceType.Planks));
            Assert.AreEqual(ironBefore + 1, _resources[0].Get(ResourceType.IronBars));
        }

        [Test]
        public void ExecuteTrade_FailsIfNotOwner()
        {
            var outpost = _tradeMap.GetOutpost("trade_planks_iron");
            outpost.TryClaim(0);
            Assert.IsFalse(_trade.ExecuteTrade(1, "trade_planks_iron"));
        }

        [Test]
        public void ExecuteTrade_FailsWithInsufficientResources()
        {
            var outpost = _tradeMap.GetOutpost("trade_planks_iron");
            outpost.TryClaim(0);
            _resources[0].Set(ResourceType.Planks, 0);
            Assert.IsFalse(_trade.ExecuteTrade(0, "trade_planks_iron"));
        }

        [Test]
        public void ExecuteTrade_FiresEvent()
        {
            string eventOutpost = null;
            _events.Subscribe<TradeExecutedEvent>(e => eventOutpost = e.OutpostId);
            var outpost = _tradeMap.GetOutpost("trade_planks_iron");
            outpost.TryClaim(0);
            _trade.ExecuteTrade(0, "trade_planks_iron");
            Assert.AreEqual("trade_planks_iron", eventOutpost);
        }

        // --- Claimed Count ---

        [Test]
        public void ClaimedCount_TracksCorrectly()
        {
            Assert.AreEqual(0, _tradeMap.GetClaimedCount(0));
            _tradeMap.GetOutpost("trade_planks_iron").TryClaim(0);
            _tradeMap.GetOutpost("trade_grain_bread").TryClaim(0);
            Assert.AreEqual(2, _tradeMap.GetClaimedCount(0));
        }
    }

    [TestFixture]
    public class TavernTests
    {
        private EventBus _events;
        private PrestigeSystem _prestige;
        private Dictionary<int, PlayerResources> _resources;
        private ArmySystem _army;
        private TavernSystem _tavern;

        [SetUp]
        public void Setup()
        {
            ArmySystem.ResetIdCounter();
            _events = new EventBus();
            _prestige = new PrestigeSystem(5, _events);
            _resources = new Dictionary<int, PlayerResources>
            {
                [0] = new PlayerResources(0, _events)
            };
            _resources[0].Set(ResourceType.Beer, 10);
            _resources[0].Set(ResourceType.Coins, 50);
            var graph = TestMapFactory.CreateSixSectorMap();
            _army = new ArmySystem(_prestige, _resources, graph, _events);
            _tavern = new TavernSystem(_resources, _army, _events);
        }

        [Test]
        public void ExchangeBeerForCoins()
        {
            int coins = _tavern.ExchangeBeerForCoins(0, 3);
            Assert.AreEqual(9, coins); // 3 beer × 3 = 9 coins
            Assert.AreEqual(7, _resources[0].Get(ResourceType.Beer));
            Assert.AreEqual(59, _resources[0].Get(ResourceType.Coins));
        }

        [Test]
        public void ExchangeCoinsForTools()
        {
            int tools = _tavern.ExchangeCoinsForTools(0, 2);
            Assert.AreEqual(2, tools);
            Assert.AreEqual(40, _resources[0].Get(ResourceType.Coins)); // 2×5=10 coins spent
        }

        [Test]
        public void ExchangeBeer_FailsWithInsufficient()
        {
            Assert.AreEqual(0, _tavern.ExchangeBeerForCoins(0, 20));
        }

        [Test]
        public void HireGeneral_CostsCoins()
        {
            int coinsBefore = _resources[0].Get(ResourceType.Coins);
            var gen = _tavern.HireGeneral(0, 0);
            Assert.IsNotNull(gen);
            Assert.AreEqual(coinsBefore - 10, _resources[0].Get(ResourceType.Coins));
        }

        [Test]
        public void HireGeneral_FailsWithInsufficientCoins()
        {
            _resources[0].Set(ResourceType.Coins, 0);
            Assert.IsNull(_tavern.HireGeneral(0, 0));
        }

        [Test]
        public void TavernExchange_FiresEvent()
        {
            ResourceType? outputRes = null;
            _events.Subscribe<TavernExchangeEvent>(e => outputRes = e.OutputResource);
            _tavern.ExchangeBeerForCoins(0, 1);
            Assert.AreEqual(ResourceType.Coins, outputRes);
        }
    }
}
