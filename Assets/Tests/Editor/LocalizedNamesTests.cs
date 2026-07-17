using System;
using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>
    /// Validates LocalizedNames and the integrity of both shipped string
    /// tables: every resource type must have a name in EN and DE, and both
    /// CSV files must parse.
    /// </summary>
    [TestFixture]
    public class LocalizedNamesTests
    {
        [TearDown]
        public void Cleanup() => L.Clear();

        [Test]
        public void EnglishTable_HasAllResourceNames()
        {
            var table = StringTablePersistence.Load("en");
            Assert.Greater(table.Count, 0, "StringTable.en.csv must load");

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                Assert.IsTrue(table.ContainsKey(LocalizedNames.ResourceKey(type)),
                    $"StringTable.en.csv missing {LocalizedNames.ResourceKey(type)}");
            }
        }

        [Test]
        public void GermanTable_HasAllResourceNames()
        {
            var table = StringTablePersistence.Load("de");
            Assert.Greater(table.Count, 0, "StringTable.de.csv must load");

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                Assert.IsTrue(table.ContainsKey(LocalizedNames.ResourceKey(type)),
                    $"StringTable.de.csv missing {LocalizedNames.ResourceKey(type)}");
            }
        }

        [Test]
        public void GermanTable_CoversEveryEnglishKey()
        {
            var en = StringTablePersistence.Load("en");
            var de = StringTablePersistence.Load("de");

            foreach (var key in en.Keys)
            {
                Assert.IsTrue(de.ContainsKey(key),
                    $"StringTable.de.csv missing key '{key}' present in en");
            }
        }

        [Test]
        public void EnglishTable_CoversEveryGermanKey()
        {
            var en = StringTablePersistence.Load("en");
            var de = StringTablePersistence.Load("de");

            foreach (var key in de.Keys)
            {
                Assert.IsTrue(en.ContainsKey(key),
                    $"StringTable.en.csv missing key '{key}' present in de");
            }
        }

        [Test]
        public void Resource_UsesLoadedLocale_AndFallsBackToEnumName()
        {
            L.SetLocale("de");
            Assert.AreEqual("Holz", LocalizedNames.Resource(ResourceType.Wood));
            Assert.AreEqual("Eisenbarren", LocalizedNames.Resource(ResourceType.IronBars));

            L.SetLocale("en");
            Assert.AreEqual("Wood", LocalizedNames.Resource(ResourceType.Wood));

            L.Clear();
            Assert.AreEqual("Wood", LocalizedNames.Resource(ResourceType.Wood),
                "Without a table the enum name is the fallback");
        }

        [Test]
        public void EnglishTable_HasAllRecipeTechAndPrestigeKeys()
        {
            var en = StringTablePersistence.Load("en");

            foreach (var recipe in RecipeDatabase.All)
                Assert.IsTrue(en.ContainsKey(LocalizedNames.RecipeKey(recipe.WorkYardId)),
                    $"StringTable.en.csv missing {LocalizedNames.RecipeKey(recipe.WorkYardId)}");

            foreach (var tech in TechTree.All)
            {
                Assert.IsTrue(en.ContainsKey(LocalizedNames.TechNameKey(tech.Id)),
                    $"StringTable.en.csv missing {LocalizedNames.TechNameKey(tech.Id)}");
                Assert.IsTrue(en.ContainsKey(LocalizedNames.TechDescKey(tech.Id)),
                    $"StringTable.en.csv missing {LocalizedNames.TechDescKey(tech.Id)}");
            }

            foreach (var unlock in PrestigeDatabase.All)
            {
                Assert.IsTrue(en.ContainsKey(LocalizedNames.PrestigeNameKey(unlock.Id)),
                    $"StringTable.en.csv missing {LocalizedNames.PrestigeNameKey(unlock.Id)}");
                Assert.IsTrue(en.ContainsKey(LocalizedNames.PrestigeDescKey(unlock.Id)),
                    $"StringTable.en.csv missing {LocalizedNames.PrestigeDescKey(unlock.Id)}");
            }
        }

        [Test]
        public void EnglishTable_HasAllOutpostKeys_ForEveryTradeMap()
        {
            var en = StringTablePersistence.Load("en");
            var maps = new[]
            {
                TestTradeMapFactory.CreateTestTradeMap(),
                TestTradeMapFactory.CreateLargeValleyTradeMap(),
                FourPlayerTradeMapFactory.CreateCrownWarTradeMap(),
                FourPlayerTradeMapFactory.CreateEmpireTradeMap(),
                SkirmishTradeMapFactory.CreateHighlandDuelTradeMap(),
                SkirmishTradeMapFactory.CreateGoldenMeadowsTradeMap(),
                SkirmishTradeMapFactory.CreateTheFrontierTradeMap()
            };

            foreach (var map in maps)
                foreach (var outpost in map.AllOutposts)
                    Assert.IsTrue(en.ContainsKey(LocalizedNames.OutpostKey(outpost.Id)),
                        $"StringTable.en.csv missing {LocalizedNames.OutpostKey(outpost.Id)}");
        }

        [Test]
        public void DatabaseNames_UseLoadedLocale_AndFallBackToDisplayName()
        {
            L.SetLocale("de");
            Assert.AreEqual("Bäckerei", LocalizedNames.Recipe("bakery"));
            Assert.AreEqual("Fischerei", LocalizedNames.Tech("tech_fishing"));
            Assert.AreEqual("Festung", LocalizedNames.Prestige("mil_stronghold"));
            var outpost = TestTradeMapFactory.CreateTestTradeMap()
                .GetOutpost("trade_grain_bread");
            Assert.AreEqual("Bäcker-Kontor", LocalizedNames.Outpost(outpost));

            L.Clear();
            Assert.AreEqual("Bakery", LocalizedNames.Recipe("bakery"),
                "Without a table the EN DisplayName is the fallback");
            Assert.AreEqual("Fishing Nets", LocalizedNames.Tech("tech_fishing"));
            Assert.AreEqual("Stronghold", LocalizedNames.Prestige("mil_stronghold"));
            Assert.AreEqual("Baker's Exchange", LocalizedNames.Outpost(outpost));
        }

        [Test]
        public void VerifiedGermanStrings_MatchOriginal()
        {
            var de = StringTablePersistence.Load("de");

            // CLAUDE.md §14.1 — verified 1:1 from original screenshots
            Assert.AreEqual("BAUEN", de["ui.build.menu_title"]);
            Assert.AreEqual("PRESTIGE-OPTIONEN", de["ui.prestige.title"]);
            Assert.AreEqual("BELOHNUNGEN", de["ui.reward.title"]);
            Assert.AreEqual("ÜBERSICHT", de["ui.stats.title"]);
            Assert.AreEqual("Produktionsübersicht: {0}", de["ui.stats.production"]);
            Assert.AreEqual("ERFORDERT", de["ui.stats.col.requires"]);
            Assert.AreEqual("PRODUZIERT VON", de["ui.stats.col.produced_by"]);
            Assert.AreEqual("ERBRINGT", de["ui.stats.col.yields"]);
            Assert.AreEqual("VERBRAUCHT VON", de["ui.stats.col.consumed_by"]);
            Assert.AreEqual("Einige Güter fehlen. Ich muss warten.",
                de["ui.carrier.waiting"]);
            Assert.AreEqual("Wer trödelt denn da? Ihr verschwendet meine Zeit!",
                de["ui.carrier.impatient"]);
            Assert.AreEqual("Bevölkerungsbelohnung", de["ui.reward.population"]);
            Assert.AreEqual("Eroberungsbelohnung", de["ui.reward.conquest"]);

            // CLAUDE.md §14.2 — verified VP names
            Assert.AreEqual("Wunderkind", de["ui.vp.vp_genius"]);
            Assert.AreEqual("Quelle der Weisheit", de["ui.vp.vp_fountain"]);
            Assert.AreEqual("Generalissimus", de["ui.vp.vp_generalissimo"]);
            Assert.AreEqual("Sonnenkönig", de["ui.vp.vp_sun_king"]);
            Assert.AreEqual("Handelsgesellschaft", de["ui.vp.vp_trading_company"]);
            Assert.AreEqual("Sparfuchs", de["ui.vp.vp_banker"]);
            Assert.AreEqual("Imperator", de["ui.vp.vp_emperor"]);
            Assert.AreEqual("Metropole", de["ui.vp.vp_metropolis"]);
            Assert.AreEqual("Handelsaußenposten", de["ui.vp.special_outpost"]);
            Assert.AreEqual("Spezieller Sektor", de["ui.vp.special_sector"]);

            // §14.9 goods (spot checks incl. the Sprint-7c additions)
            Assert.AreEqual("Fleisch", de["ui.res.meat"]);
            Assert.AreEqual("Gewürz", de["ui.res.spice"]);
            Assert.AreEqual("Wein", de["ui.res.wine"]);
        }

        [Test]
        public void EnglishTable_HasAllMissionKeys()
        {
            var en = StringTablePersistence.Load("en");
            foreach (var mission in CampaignSystem.AllMissions)
            {
                Assert.IsTrue(en.ContainsKey($"ui.mission.{mission.Id}.title"),
                    $"missing ui.mission.{mission.Id}.title");
                Assert.IsTrue(en.ContainsKey($"ui.mission.{mission.Id}.briefing"),
                    $"missing ui.mission.{mission.Id}.briefing");
                for (int i = 0; i < mission.Objectives.Length; i++)
                    Assert.IsTrue(en.ContainsKey($"ui.mission.{mission.Id}.obj{i}"),
                        $"missing ui.mission.{mission.Id}.obj{i}");
            }
        }
    }
}
