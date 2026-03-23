using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tavern functions: exchange beer for coins, coins for tools,
    /// recruit settlers, hire generals.
    /// Taverns are predefined economy buildings found in specific sectors.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class TavernSystem
    {
        private readonly Dictionary<int, PlayerResources> _resources;
        private readonly ArmySystem _army;
        private readonly EventBus _eventBus;

        // Exchange rates
        private const int BEER_TO_COINS_RATE = 3;  // 1 beer → 3 coins
        private const int COINS_TO_TOOLS_RATE = 5;  // 5 coins → 1 tool

        public TavernSystem(Dictionary<int, PlayerResources> resources,
            ArmySystem army, EventBus eventBus)
        {
            _resources = resources;
            _army = army;
            _eventBus = eventBus;
        }

        /// <summary>Exchange beer for coins. Returns number of coins gained.</summary>
        public int ExchangeBeerForCoins(int playerId, int beerCount)
        {
            if (!_resources.TryGetValue(playerId, out var res))
                return 0;
            if (!res.Has(ResourceType.Beer, beerCount))
                return 0;

            res.TrySpend(ResourceType.Beer, beerCount);
            int coins = beerCount * BEER_TO_COINS_RATE;
            res.Add(ResourceType.Coins, coins);

            _eventBus.Publish(new TavernExchangeEvent(
                playerId, ResourceType.Beer, beerCount,
                ResourceType.Coins, coins));
            return coins;
        }

        /// <summary>Exchange coins for tools. Returns number of tools gained.</summary>
        public int ExchangeCoinsForTools(int playerId, int toolCount)
        {
            if (!_resources.TryGetValue(playerId, out var res))
                return 0;

            int coinCost = toolCount * COINS_TO_TOOLS_RATE;
            if (!res.Has(ResourceType.Coins, coinCost))
                return 0;

            res.TrySpend(ResourceType.Coins, coinCost);
            res.Add(ResourceType.Tools, toolCount);

            _eventBus.Publish(new TavernExchangeEvent(
                playerId, ResourceType.Coins, coinCost,
                ResourceType.Tools, toolCount));
            return toolCount;
        }

        /// <summary>Hire a general at the tavern (costs coins).</summary>
        public General HireGeneral(int playerId, int sectorId, int coinCost = 10)
        {
            if (!_resources.TryGetValue(playerId, out var res))
                return null;
            if (!res.Has(ResourceType.Coins, coinCost))
                return null;

            var general = _army.HireGeneral(playerId, sectorId);
            if (general == null) return null;

            res.TrySpend(ResourceType.Coins, coinCost);
            return general;
        }
    }

    public readonly struct TavernExchangeEvent
    {
        public readonly int PlayerId;
        public readonly ResourceType InputResource;
        public readonly int InputAmount;
        public readonly ResourceType OutputResource;
        public readonly int OutputAmount;

        public TavernExchangeEvent(int playerId,
            ResourceType inputRes, int inputAmt,
            ResourceType outputRes, int outputAmt)
        {
            PlayerId = playerId;
            InputResource = inputRes;
            InputAmount = inputAmt;
            OutputResource = outputRes;
            OutputAmount = outputAmt;
        }
    }
}
