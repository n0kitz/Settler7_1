using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>Predefined quests for the test maps.</summary>
    public static class QuestDatabase
    {
        public static List<Quest> GetQuestsForMap(string mapId)
        {
            var quests = new List<Quest>();
            AddUniversalQuests(quests);

            switch (mapId)
            {
                case "large_valley":
                    AddLargeValleyQuests(quests);
                    break;
                case "crown_war":
                    AddCrownWarQuests(quests);
                    break;
                case "empire":
                    AddEmpireQuests(quests);
                    break;
                default:
                    AddTestValleyQuests(quests);
                    break;
            }

            return quests;
        }

        private static void AddUniversalQuests(List<Quest> quests)
        {
            var q1 = new Quest("quest_lumber_baron", "Lumber Baron",
                "Deliver 20 Planks to prove your logging prowess.", -1);
            q1.Objectives.Add(QuestObjective.Deliver(ResourceType.Planks, 20));
            q1.Rewards.Add(QuestReward.Prestige(3));
            q1.Rewards.Add(QuestReward.Resource(ResourceType.Tools, 5));
            quests.Add(q1);

            var q2 = new Quest("quest_iron_will", "Iron Will",
                "Deliver 10 Iron Bars to the forge.", -1);
            q2.Objectives.Add(QuestObjective.Deliver(ResourceType.IronBars, 10));
            q2.Rewards.Add(QuestReward.Resource(ResourceType.Weapons, 5));
            q2.Rewards.Add(QuestReward.Prestige(2));
            quests.Add(q2);

            var q3 = new Quest("quest_expansionist", "Expansionist",
                "Control 4 sectors to demonstrate your reach.", -1);
            q3.Objectives.Add(QuestObjective.OwnSectors(4));
            q3.Rewards.Add(QuestReward.VP("vp_quest_expansionist"));
            quests.Add(q3);

            var q4 = new Quest("quest_scholar", "Scholar",
                "Research any technology.", -1);
            q4.Objectives.Add(QuestObjective.Tech("tech_plowing"));
            q4.Rewards.Add(QuestReward.Prestige(2));
            q4.Rewards.Add(QuestReward.Resource(ResourceType.Books, 3));
            quests.Add(q4);
        }

        private static void AddTestValleyQuests(List<Quest> quests)
        {
            var q5 = new Quest("quest_bread_basket", "Bread Basket",
                "Deliver 15 Bread to feed the hungry.", -1);
            q5.Objectives.Add(QuestObjective.Deliver(ResourceType.Bread, 15));
            q5.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 20));
            quests.Add(q5);

            var q6 = new Quest("quest_warlord", "Warlord",
                "Raise an army of 15 soldiers.", -1);
            q6.Objectives.Add(QuestObjective.Army(15));
            q6.Rewards.Add(QuestReward.VP("vp_quest_warlord"));
            quests.Add(q6);
        }

        private static void AddLargeValleyQuests(List<Quest> quests)
        {
            // Map-specific quests tied to sectors
            var q1 = new Quest("quest_gold_rush", "Gold Rush",
                "Deliver 10 Gold Ore from the Dragon's Peak mines.", 11);
            q1.Objectives.Add(QuestObjective.Deliver(ResourceType.GoldOre, 10));
            q1.Rewards.Add(QuestReward.VP("vp_quest_gold_rush"));
            q1.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 30));
            quests.Add(q1);

            var q2 = new Quest("quest_iron_lord", "Iron Lord",
                "Control the Iron Hills and deliver 15 Iron Bars.", 4);
            q2.Objectives.Add(QuestObjective.Deliver(ResourceType.IronBars, 15));
            q2.Rewards.Add(QuestReward.Prestige(5));
            q2.Rewards.Add(QuestReward.Resource(ResourceType.Weapons, 8));
            quests.Add(q2);

            var q3 = new Quest("quest_breadbasket_lv", "Breadbasket of the Valley",
                "Deliver 20 Bread from the Southern Farmlands.", 8);
            q3.Objectives.Add(QuestObjective.Deliver(ResourceType.Bread, 20));
            q3.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 25));
            q3.Rewards.Add(QuestReward.Prestige(3));
            quests.Add(q3);

            var q4 = new Quest("quest_conqueror", "Conqueror",
                "Control 6 sectors to dominate the valley.", -1);
            q4.Objectives.Add(QuestObjective.OwnSectors(6));
            q4.Rewards.Add(QuestReward.VP("vp_quest_conqueror"));
            quests.Add(q4);

            var q5 = new Quest("quest_master_trader", "Master Trader",
                "Deliver 20 Coins to prove your trade mastery.", -1);
            q5.Objectives.Add(QuestObjective.Deliver(ResourceType.Coins, 20));
            q5.Rewards.Add(QuestReward.Prestige(4));
            q5.Rewards.Add(QuestReward.Resource(ResourceType.Jewelry, 5));
            quests.Add(q5);

            var q6 = new Quest("quest_grand_army", "Grand Army",
                "Raise an army of 25 soldiers.", -1);
            q6.Objectives.Add(QuestObjective.Army(25));
            q6.Rewards.Add(QuestReward.VP("vp_quest_grand_army"));
            quests.Add(q6);
        }
        private static void AddCrownWarQuests(List<Quest> quests)
        {
            var q1 = new Quest("quest_cw_king_road", "King's Road",
                "Control the King's Crossroads.", 12);
            q1.Objectives.Add(QuestObjective.OwnSectors(5));
            q1.Rewards.Add(QuestReward.VP("vp_quest_king_road"));
            quests.Add(q1);

            var q2 = new Quest("quest_cw_dragon_mine", "Dragon's Bounty",
                "Deliver 15 Gold Ore from the Dragon's Mine.", 15);
            q2.Objectives.Add(QuestObjective.Deliver(ResourceType.GoldOre, 15));
            q2.Rewards.Add(QuestReward.VP("vp_quest_dragon_bounty"));
            q2.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 30));
            quests.Add(q2);

            var q3 = new Quest("quest_cw_warlord", "Crown Warlord",
                "Raise an army of 20 soldiers.", -1);
            q3.Objectives.Add(QuestObjective.Army(20));
            q3.Rewards.Add(QuestReward.VP("vp_quest_crown_warlord"));
            quests.Add(q3);

            var q4 = new Quest("quest_cw_market", "Market Master",
                "Deliver 25 Coins to the Grand Market.", 14);
            q4.Objectives.Add(QuestObjective.Deliver(ResourceType.Coins, 25));
            q4.Rewards.Add(QuestReward.Prestige(4));
            q4.Rewards.Add(QuestReward.Resource(ResourceType.Jewelry, 5));
            quests.Add(q4);

            var q5 = new Quest("quest_cw_fortifier", "Master Fortifier",
                "Deliver 30 Stone to fortify your realm.", -1);
            q5.Objectives.Add(QuestObjective.Deliver(ResourceType.Stone, 30));
            q5.Rewards.Add(QuestReward.Prestige(3));
            q5.Rewards.Add(QuestReward.Resource(ResourceType.Tools, 8));
            quests.Add(q5);

            var q6 = new Quest("quest_cw_bread", "Four Kingdoms Feast",
                "Deliver 25 Bread.", -1);
            q6.Objectives.Add(QuestObjective.Deliver(ResourceType.Bread, 25));
            q6.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 20));
            q6.Rewards.Add(QuestReward.Prestige(2));
            quests.Add(q6);
        }

        private static void AddEmpireQuests(List<Quest> quests)
        {
            var q1 = new Quest("quest_emp_throne", "Claim the Throne",
                "Control the Throne of Kings.", 20);
            q1.Objectives.Add(QuestObjective.OwnSectors(7));
            q1.Rewards.Add(QuestReward.VP("vp_quest_throne"));
            quests.Add(q1);

            var q2 = new Quest("quest_emp_gold_hoard", "Dragon's Hoard",
                "Deliver 20 Gold Ore.", 22);
            q2.Objectives.Add(QuestObjective.Deliver(ResourceType.GoldOre, 20));
            q2.Rewards.Add(QuestReward.VP("vp_quest_gold_hoard"));
            q2.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 40));
            quests.Add(q2);

            var q3 = new Quest("quest_emp_grand_army", "Imperial Army",
                "Raise an army of 30 soldiers.", -1);
            q3.Objectives.Add(QuestObjective.Army(30));
            q3.Rewards.Add(QuestReward.VP("vp_quest_imperial_army"));
            quests.Add(q3);

            var q4 = new Quest("quest_emp_library", "Grand Scholar",
                "Deliver 10 Books to the Ancient Library.", 21);
            q4.Objectives.Add(QuestObjective.Deliver(ResourceType.Books, 10));
            q4.Rewards.Add(QuestReward.Prestige(5));
            q4.Rewards.Add(QuestReward.Resource(ResourceType.Coins, 20));
            quests.Add(q4);

            var q5 = new Quest("quest_emp_conqueror", "Imperial Conqueror",
                "Control 8 sectors.", -1);
            q5.Objectives.Add(QuestObjective.OwnSectors(8));
            q5.Rewards.Add(QuestReward.VP("vp_quest_imperial_conqueror"));
            quests.Add(q5);

            var q6 = new Quest("quest_emp_weaponsmith", "Master Weaponsmith",
                "Deliver 15 Weapons.", -1);
            q6.Objectives.Add(QuestObjective.Deliver(ResourceType.Weapons, 15));
            q6.Rewards.Add(QuestReward.Prestige(4));
            q6.Rewards.Add(QuestReward.Resource(ResourceType.Horses, 5));
            quests.Add(q6);

            var q7 = new Quest("quest_emp_merchant", "Merchant Prince",
                "Deliver 30 Coins.", -1);
            q7.Objectives.Add(QuestObjective.Deliver(ResourceType.Coins, 30));
            q7.Rewards.Add(QuestReward.Resource(ResourceType.Jewelry, 8));
            q7.Rewards.Add(QuestReward.Prestige(3));
            quests.Add(q7);

            var q8 = new Quest("quest_emp_feast", "Imperial Feast",
                "Deliver 20 Sausages.", -1);
            q8.Objectives.Add(QuestObjective.Deliver(ResourceType.Sausages, 20));
            q8.Rewards.Add(QuestReward.Resource(ResourceType.Beer, 15));
            q8.Rewards.Add(QuestReward.Prestige(2));
            quests.Add(q8);
        }
    }
}

