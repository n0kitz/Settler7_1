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
        public void VerifiedGermanStrings_MatchOriginal()
        {
            var de = StringTablePersistence.Load("de");
            // CLAUDE.md §14.1 — verified 1:1 from original screenshots
            Assert.AreEqual("BAUEN", de["ui.build.menu_title"]);
            Assert.AreEqual("PRESTIGE-OPTIONEN", de["ui.prestige.title"]);
            Assert.AreEqual("BELOHNUNGEN", de["ui.reward.title"]);
            Assert.AreEqual("Einige Güter fehlen. Ich muss warten.",
                de["ui.carrier.waiting"]);
            Assert.AreEqual("Wunderkind", de["ui.vp.vp_genius"]);
        }
    }
}
