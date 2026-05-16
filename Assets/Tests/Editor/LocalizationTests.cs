using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using Settlers.Simulation;

namespace Settlers.Tests
{
    /// <summary>Tests for LocalizationDatabase, StringTablePersistence, and KeyBindings.</summary>
    [TestFixture]
    public class LocalizationTests
    {
        [TearDown]
        public void Cleanup()
        {
            L.Clear();
        }

        // --- L (LocalizationDatabase) ---

        [Test]
        public void Get_WithSeedData_ReturnsString()
        {
            L.Seed(new Dictionary<string, string>
            {
                { "ui.main_menu.new_game", "New Game" }
            });
            Assert.AreEqual("New Game", L.Get("ui.main_menu.new_game"));
        }

        [Test]
        public void Get_MissingKey_ReturnsKey()
        {
            L.Seed(new Dictionary<string, string>());
            Assert.AreEqual("missing.key", L.Get("missing.key"));
        }

        [Test]
        public void Has_ExistingKey_ReturnsTrue()
        {
            L.Seed(new Dictionary<string, string> { { "x", "y" } });
            Assert.IsTrue(L.Has("x"));
        }

        [Test]
        public void Has_MissingKey_ReturnsFalse()
        {
            L.Seed(new Dictionary<string, string>());
            Assert.IsFalse(L.Has("missing"));
        }

        [Test]
        public void SetLocale_SetsCurrentLocale()
        {
            L.SetLocale("de");
            Assert.AreEqual("de", L.CurrentLocale);
            L.SetLocale("en"); // reset
        }

        [Test]
        public void Clear_RemovesAllStrings()
        {
            L.Seed(new Dictionary<string, string> { { "k", "v" } });
            L.Clear();
            Assert.AreEqual("k", L.Get("k"), "Should return key when cleared");
        }

        // --- StringTablePersistence ---

        [Test]
        public void StringTable_Load_MissingFile_ReturnsEmpty()
        {
            var result = StringTablePersistence.Load("zz");  // no such locale
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void StringTable_SaveAndLoad_RoundTrips()
        {
            const string LOCALE = "test_locale_tmp";
            var original = new Dictionary<string, string>
            {
                { "hello", "world" },
                { "foo",   "bar"   },
            };
            StringTablePersistence.Save(LOCALE, original);
            var loaded = StringTablePersistence.Load(LOCALE);

            Assert.AreEqual("world", loaded["hello"]);
            Assert.AreEqual("bar",   loaded["foo"]);

            // Cleanup test file
            var path = Path.Combine("Assets/Resources/Localization",
                $"StringTable.{LOCALE}.csv");
            if (File.Exists(path)) File.Delete(path);
        }

        [Test]
        public void StringTable_IgnoresCommentLines()
        {
            const string LOCALE = "test_comments_tmp";
            StringTablePersistence.Save(LOCALE,
                new Dictionary<string, string> { { "key", "val" } });

            // Prepend comment line to saved file
            var path = Path.Combine("Assets/Resources/Localization",
                $"StringTable.{LOCALE}.csv");
            var lines = File.ReadAllLines(path);
            var withComment = new List<string>(lines) { "# this is a comment" };
            File.WriteAllLines(path, withComment);

            var loaded = StringTablePersistence.Load(LOCALE);
            Assert.AreEqual("val", loaded["key"]);
            Assert.IsFalse(loaded.ContainsKey("# this is a comment"));

            if (File.Exists(path)) File.Delete(path);
        }

        // --- KeyBindings ---

        [Test]
        public void KeyBindings_Default_HasExpectedActions()
        {
            var kb = KeyBindings.Default;
            Assert.AreEqual("Q", kb.Get("toggle_quest"));
            Assert.AreEqual("T", kb.Get("toggle_tech"));
        }

        [Test]
        public void KeyBindings_Set_OverridesBinding()
        {
            var kb = new KeyBindings();
            kb.Set("toggle_quest", "Z");
            Assert.AreEqual("Z", kb.Get("toggle_quest"));
        }

        [Test]
        public void KeyBindings_ResetAction_RestoresDefault()
        {
            var kb = new KeyBindings();
            kb.Set("toggle_quest", "Z");
            kb.ResetAction("toggle_quest");
            Assert.AreEqual("Q", kb.Get("toggle_quest"));
        }

        [Test]
        public void KeyBindings_ResetAll_RestoresAllDefaults()
        {
            var kb = new KeyBindings();
            kb.Set("toggle_quest", "Z");
            kb.Set("toggle_tech",  "X");
            kb.ResetAll();
            Assert.AreEqual("Q", kb.Get("toggle_quest"));
            Assert.AreEqual("T", kb.Get("toggle_tech"));
        }

        [Test]
        public void KeyBindings_Get_UnknownAction_ReturnsNull()
        {
            var kb = new KeyBindings();
            Assert.IsNull(kb.Get("not_an_action"));
        }

        // --- KeyBindingsPersistence ---

        [Test]
        public void KeyBindingsPersistence_SaveAndLoad_RoundTrips()
        {
            var kb = new KeyBindings();
            kb.Set("toggle_quest", "F1");
            KeyBindingsPersistence.Save(kb);

            var loaded = KeyBindingsPersistence.Load();
            Assert.AreEqual("F1", loaded.Get("toggle_quest"));
        }
    }
}
