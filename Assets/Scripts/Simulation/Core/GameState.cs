using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Master state container for the entire simulation.
    /// Owns all systems and per-player state.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class GameState
    {
        /// <summary>Sector adjacency graph.</summary>
        public SectorGraph Graph { get; }

        /// <summary>Event bus for simulation events.</summary>
        public EventBus Events { get; }

        /// <summary>Per-player resource inventories.</summary>
        public Dictionary<int, PlayerResources> PlayerResources { get; }

        // --- Economy Systems ---
        public ConstructionSystem Construction { get; }
        public ProductionSystem Production { get; }
        public PopulationSystem Population { get; }
        public LogisticsSystem Logistics { get; }
        public UpgradeSystem Upgrades { get; }

        // --- Meta Systems ---
        public PrestigeSystem Prestige { get; }

        // --- Military Systems ---
        public ArmySystem Army { get; }
        public CombatResolver Combat { get; }
        public ConquestSystem Conquest { get; }
        public FortificationSystem Fortification { get; }

        // --- Technology Systems ---
        public ResearchSystem Research { get; }
        public TechEffects TechEffects { get; }

        // --- Trade Systems ---
        public TradeSystem Trade { get; }
        public TavernSystem Tavern { get; }
        public TradeMap TradeMapData { get; }

        // --- Victory + AI + Quests ---
        public VictorySystem Victory { get; }
        public QuestSystem Quests { get; }
        public List<AIController> AIPlayers { get; }

        /// <summary>Number of players in this game.</summary>
        public int PlayerCount { get; }

        /// <summary>Current simulation time in seconds.</summary>
        public float SimulationTime { get; private set; }

        /// <summary>Map ID used to create this game.</summary>
        public string MapId { get; }

        public GameState(SectorGraph graph, int playerCount,
            float constructionBaseTime, int carrierMaxItems,
            int vpRequired = 4, string mapId = "test_valley",
            float countdownDuration = 180f, VPThresholds vpThresholds = null)
        {
            Graph = graph;
            PlayerCount = playerCount;
            MapId = mapId;
            Events = new EventBus();

            // Per-player resources
            PlayerResources = new Dictionary<int, PlayerResources>();
            for (int p = 0; p < playerCount; p++)
            {
                var res = new PlayerResources(p, Events);
                res.Set(ResourceType.Planks, 20);
                res.Set(ResourceType.Stone, 10);
                res.Set(ResourceType.Tools, 5);
                PlayerResources[p] = res;
            }

            // Economy systems
            Construction = new ConstructionSystem(Events, constructionBaseTime);
            Production = new ProductionSystem(PlayerResources, Construction, Events);
            Population = new PopulationSystem(PlayerResources, Construction, Production, Events);
            Logistics = new LogisticsSystem(graph, carrierMaxItems, Events);
            Prestige = new PrestigeSystem(pointsPerLevel: 5, Events);
            Upgrades = new UpgradeSystem(Construction, PlayerResources,
                Prestige, Events, constructionBaseTime);

            // Military systems
            Army = new ArmySystem(Prestige, PlayerResources, graph, Events);
            Combat = new CombatResolver(graph, Events);
            Conquest = new ConquestSystem(graph, Army, Combat, PlayerResources, Events);
            Fortification = new FortificationSystem(graph, Prestige, PlayerResources, Events);

            // Technology systems
            Research = new ResearchSystem(Events);
            TechEffects = new TechEffects(Research);
            Production.SetTechEffects(TechEffects);
            Production.SetPrestige(Prestige);
            Construction.SetTechEffects(TechEffects);
            Population.SetTechEffects(TechEffects);
            Logistics.SetTechEffects(TechEffects);
            Combat.SetTechEffects(TechEffects);
            Fortification.SetTechEffects(TechEffects);

            // Trade systems
            TradeMapData = TestTradeMapFactory.CreateForMap(mapId);
            Trade = new TradeSystem(TradeMapData, PlayerResources, Prestige, Events);
            Tavern = new TavernSystem(PlayerResources, Army, Events);

            // Each player starts with 1 constructor
            for (int p = 0; p < playerCount; p++)
                Construction.SetConstructorCount(p, 1);

            // Place storehouses in starting sectors
            for (int i = 0; i < graph.SectorCount; i++)
            {
                var sector = graph.GetSector(i);
                if (sector.IsPlayerOwned)
                    Logistics.PlaceStorehouse(i, sector.OwnerId);
            }

            // Victory system
            Victory = new VictorySystem(this, vpRequired, countdownDuration, vpThresholds);

            // Quest system
            Quests = new QuestSystem(this);
            foreach (var quest in QuestDatabase.GetQuestsForMap(mapId))
                Quests.AddQuest(quest);

            // AI players (all non-zero players are AI)
            AIPlayers = new List<AIController>();
            for (int p = 1; p < playerCount; p++)
                AIPlayers.Add(new AIController(this, p));

            // Wire sector conquest → prestige award (+1 per sector) + permanent VP
            Events.Subscribe<SectorConqueredEvent>(OnSectorConquered);

            // Wire tech researched → permanent VP (Genius: first to complete all tier 3)
            Events.Subscribe<TechResearchedEvent>(OnTechResearched);

            // Wire trade outpost claimed → permanent VP for special outposts
            Events.Subscribe<OutpostClaimedEvent>(OnOutpostClaimed);

            // Wire carrier delivery → deposit goods at destination storehouse owner
            Events.Subscribe<CarrierDeliveryEvent>(OnCarrierDelivery);
        }

        private void OnSectorConquered(SectorConqueredEvent evt)
        {
            Prestige.AwardPoints(evt.NewOwnerId, 1);

            // Destroy all buildings belonging to the previous owner in this sector
            if (evt.PreviousOwnerId >= 0)
            {
                var destroyed = Construction.RemoveBuildingsInSector(evt.SectorId, evt.PreviousOwnerId);
                foreach (int buildingId in destroyed)
                {
                    Production.UnregisterWorkYardsForBuilding(buildingId);
                    Events.Publish(new BuildingDestroyedEvent(buildingId, evt.SectorId));
                }
            }

            // Replace storehouse with one owned by the new player
            Logistics.ReplaceStorehouse(evt.SectorId, evt.NewOwnerId);

            // Special sector VP
            var sector = Graph.GetSector(evt.SectorId);
            if (sector.VPRewardId != null)
                Victory.AwardPermanentVP(evt.NewOwnerId, sector.VPRewardId);
        }

        private void OnTechResearched(TechResearchedEvent evt)
        {
            // Genius VP: first player to complete all 6 tier-3 techs
            int tier3Count = 0;
            foreach (var tech in TechTree.All)
                if (tech.Tier == TechTree.TechTier.Tier3 && Research.HasTech(evt.PlayerId, tech.Id))
                    tier3Count++;
            if (tier3Count >= 6)
                Victory.AwardPermanentVP(evt.PlayerId, "vp_genius");
        }

        private void OnOutpostClaimed(OutpostClaimedEvent evt)
        {
            if (evt.IsSpecial)
                Victory.AwardPermanentVP(evt.PlayerId, "vp_special_outpost_" + evt.OutpostId);
        }

        private void OnCarrierDelivery(CarrierDeliveryEvent evt)
        {
            // Deposit delivered goods into the destination storehouse owner's resources
            var toStorehouse = Logistics.GetStorehouse(evt.ToSectorId);
            if (toStorehouse == null) return;
            if (PlayerResources.TryGetValue(toStorehouse.OwnerId, out var res))
                res.Add(evt.ResourceType, evt.Amount);
        }

        /// <summary>Advance simulation time.</summary>
        public void AdvanceTime(float deltaTime)
        {
            SimulationTime += deltaTime;
        }
    }
}
