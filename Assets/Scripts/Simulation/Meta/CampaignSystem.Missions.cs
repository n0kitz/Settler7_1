using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// CampaignSystem — the mission catalogue. A single arc across three
    /// chapters: intro → economy → military → trade → technology → conquest →
    /// finale. Evaluation/unlock logic lives in CampaignSystem.cs.
    /// </summary>
    public partial class CampaignSystem
    {
        private static List<Mission> BuildCatalogue() => new List<Mission>
        {
            // Chapter 1: Learning the Basics
            new Mission(
                id: "c1_m1_first_steps",
                title: "First Steps",
                briefing: "My lord, our people have settled in the valley. We must build our economy " +
                          "quickly — rival lords eye our lands. Establish a Lodge and Farm, then " +
                          "expand to the neutral sectors nearby.",
                mapId: "test_valley",
                playerCount: 1,
                vpRequired: 2,
                objectives: new[]
                {
                    new MissionObjective("Reach 2 Victory Points", MissionObjectiveType.ReachVPCount, 2),
                    new MissionObjective("Own at least 3 sectors", MissionObjectiveType.ConquerSectors, 3),
                },
                unlocksNext: "c1_m2_hearth_and_home",
                chapter: 0),

            new Mission(
                id: "c1_m2_hearth_and_home",
                title: "Hearth and Home",
                briefing: "The highlands are harsh, my lord, and winter is near. Our people need " +
                          "bread before they need swords. Raise farms in the heather meadows and " +
                          "keep the ovens burning — a fed settlement is a loyal settlement.",
                mapId: "highland_duel",
                playerCount: 2,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Build 2 operational Farms", MissionObjectiveType.BuildBuilding, 2,
                        nameof(BaseBuildingType.Farm)),
                    new MissionObjective("Stockpile 15 Bread", MissionObjectiveType.ProduceResource, 15,
                        nameof(ResourceType.Bread)),
                    new MissionObjective("Own at least 4 sectors", MissionObjectiveType.ConquerSectors, 4),
                },
                startingResources: new Dictionary<ResourceType, int>
                {
                    { ResourceType.Planks, 30 },
                    { ResourceType.Stone, 15 },
                    { ResourceType.Tools, 10 },
                },
                unlocksNext: "c1_m2_iron_fist",
                chapter: 0),

            new Mission(
                id: "c1_m2_iron_fist",
                title: "Iron Fist",
                briefing: "The Iron Ridge holds resources vital to our military ambitions. " +
                          "An eastern lord has garrisoned the ridge with soldiers. " +
                          "Train your army and crush his forces. The iron belongs to us!",
                mapId: "twin_rivers",
                playerCount: 2,
                vpRequired: 4,
                objectives: new[]
                {
                    new MissionObjective("Conquer 5 sectors", MissionObjectiveType.ConquerSectors, 5),
                    new MissionObjective("Stockpile 20 Iron Bars", MissionObjectiveType.ProduceResource, 20,
                        nameof(ResourceType.IronBars)),
                },
                unlocksNext: "c1_m3_mountain_lord",
                chapter: 0),

            new Mission(
                id: "c1_m3_mountain_lord",
                title: "Lord of the Mountain",
                briefing: "Three lords vie for the Central Stronghold. He who holds it commands the pass " +
                          "and wins prestige across the realm. Take the stronghold by any means necessary.",
                mapId: "mountain_pass",
                playerCount: 3,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Reach 5 Victory Points", MissionObjectiveType.ReachVPCount, 5),
                    new MissionObjective("Own the Central Stronghold (7 sectors)", MissionObjectiveType.ConquerSectors, 7),
                },
                unlocksNext: "c2_m1_sea_roads",
                chapter: 0),

            // Chapter 2: Trade & Technology
            new Mission(
                id: "c2_m1_sea_roads",
                title: "Sea Roads",
                briefing: "The island chain is a vital trade corridor. Control its outposts and your " +
                          "merchants will profit handsomely. But a rival fleet already claims the eastern isle!",
                mapId: "island_chain",
                playerCount: 2,
                vpRequired: 4,
                objectives: new[]
                {
                    new MissionObjective("Reach 4 Victory Points", MissionObjectiveType.ReachVPCount, 4),
                    new MissionObjective("Accumulate 30 Coins", MissionObjectiveType.ProduceResource, 30,
                        nameof(ResourceType.Coins)),
                },
                unlocksNext: "c2_m2_scholars_duel",
                chapter: 1),

            new Mission(
                id: "c2_m2_scholars_duel",
                title: "The Scholars' Duel",
                briefing: "Three great academies stand empty in the large valley. " +
                          "He who researches the most technologies will be known as the wisest lord. " +
                          "Your rivals have sent their clerics — do not let them claim every monastery!",
                mapId: "large_valley",
                playerCount: 3,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Reach 5 Victory Points", MissionObjectiveType.ReachVPCount, 5),
                },
                unlocksNext: "c2_m3_meadow_fair",
                chapter: 1),

            new Mission(
                id: "c2_m3_meadow_fair",
                title: "The Meadow Fair",
                briefing: "Three realms meet at the golden meadows, and their merchants meet at the " +
                          "great fair. Gold buys loyalty faster than any sword — fill your coffers " +
                          "and let your rivals watch their markets empty.",
                mapId: "golden_meadows",
                playerCount: 3,
                vpRequired: 6,
                objectives: new[]
                {
                    new MissionObjective("Accumulate 40 Coins", MissionObjectiveType.ProduceResource, 40,
                        nameof(ResourceType.Coins)),
                    new MissionObjective("Reach 6 Victory Points", MissionObjectiveType.ReachVPCount, 6),
                },
                unlocksNext: "c3_m1_crown_war",
                chapter: 1),

            // Chapter 3: Conquest
            new Mission(
                id: "c3_m1_crown_war",
                title: "The Crown War",
                briefing: "Four noble houses stand at the brink of all-out war. Only one can wear the " +
                          "crown. Forge your empire through blood, trade, or wisdom — but win you must. " +
                          "The realm watches.",
                mapId: "crown_war",
                playerCount: 4,
                vpRequired: 5,
                objectives: new[]
                {
                    new MissionObjective("Reach 5 Victory Points", MissionObjectiveType.ReachVPCount, 5),
                },
                unlocksNext: "c3_m2_empire",
                chapter: 2),

            new Mission(
                id: "c3_m2_empire",
                title: "Empire",
                briefing: "This is the final battle for dominion over the entire realm. " +
                          "24 sectors, 4 lords, and only one throne. Conquer, trade, or out-think your " +
                          "rivals. The empire is yours to claim — if you are worthy.",
                mapId: "empire",
                playerCount: 4,
                vpRequired: 6,
                objectives: new[]
                {
                    new MissionObjective("Reach 6 Victory Points", MissionObjectiveType.ReachVPCount, 6),
                    new MissionObjective("Survive 600 seconds", MissionObjectiveType.SurviveTime, 600),
                },
                unlocksNext: "c3_m3_last_frontier",
                chapter: 2),

            new Mission(
                id: "c3_m3_last_frontier",
                title: "The Last Frontier",
                briefing: "Beyond the old borders lies the frontier — forty sectors of wilderness, " +
                          "and three lords who will never kneel. There is no throne to inherit here; " +
                          "whatever kingdom rises from this land, you must build it yourself.",
                mapId: "the_frontier",
                playerCount: 4,
                vpRequired: 7,
                objectives: new[]
                {
                    new MissionObjective("Reach 7 Victory Points", MissionObjectiveType.ReachVPCount, 7),
                    new MissionObjective("Own at least 12 sectors", MissionObjectiveType.ConquerSectors, 12),
                    new MissionObjective("Survive 900 seconds", MissionObjectiveType.SurviveTime, 900),
                },
                chapter: 2),
        };
    }
}
