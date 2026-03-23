using NUnit.Framework;
using Settlers.Simulation;

namespace Settlers.Tests
{
    [TestFixture]
    public class TechEffectsTests
    {
        private EventBus _events;
        private ResearchSystem _research;
        private TechEffects _effects;

        [SetUp]
        public void Setup()
        {
            _events = new EventBus();
            _research = new ResearchSystem(_events);
            _effects = new TechEffects(_research);
        }

        [Test]
        public void NoTech_ReturnsBaseMultiplier()
        {
            Assert.AreEqual(1f, _effects.GetProductionMultiplier(0, "grain_barn"));
            Assert.AreEqual(1f, _effects.GetProductionMultiplier(0, "quarry"));
            Assert.AreEqual(1f, _effects.GetConstructionSpeedMultiplier(0));
            Assert.AreEqual(1f, _effects.GetCarrierSpeedMultiplier(0));
        }

        [Test]
        public void Plowing_BoostsGrainBarn()
        {
            ResearchInstantly(0, "tech_plowing");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "grain_barn"), 0.01f);
        }

        [Test]
        public void Masonry_BoostsQuarry()
        {
            ResearchInstantly(0, "tech_masonry");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "quarry"), 0.01f);
        }

        [Test]
        public void Carpentry_BoostsSawmill()
        {
            ResearchInstantly(0, "tech_carpentry");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "sawmill"), 0.01f);
        }

        [Test]
        public void Smelting_BoostsIronSmelter()
        {
            ResearchInstantly(0, "tech_smelting");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "iron_smelter"), 0.01f);
        }

        [Test]
        public void Fishing_BoostsFisher()
        {
            ResearchInstantly(0, "tech_fishing");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "fisher"), 0.01f);
        }

        [Test]
        public void AnimalHusbandry_BoostsPiggeryAndShepherd()
        {
            ResearchInstantly(0, "tech_animal_husbandry");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "piggery"), 0.01f);
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "shepherd"), 0.01f);
        }

        [Test]
        public void Steel_BoostsBlacksmith()
        {
            ResearchInstantly(0, "tech_smelting");
            ResearchInstantly(0, "tech_steel");
            Assert.AreEqual(1.5f, _effects.GetProductionMultiplier(0, "blacksmith"), 0.01f);
        }

        [Test]
        public void Architecture_DoublesConstructionSpeed()
        {
            ResearchInstantly(0, "tech_masonry");
            ResearchInstantly(0, "tech_fortification_tech");
            ResearchInstantly(0, "tech_architecture");
            Assert.AreEqual(2f, _effects.GetConstructionSpeedMultiplier(0), 0.01f);
        }

        [Test]
        public void Logistics_BoostsCarrierSpeed()
        {
            ResearchInstantly(0, "tech_carpentry");
            ResearchInstantly(0, "tech_woodworking");
            ResearchInstantly(0, "tech_logistics");
            Assert.AreEqual(1.5f, _effects.GetCarrierSpeedMultiplier(0), 0.01f);
        }

        [Test]
        public void Cavalry_BoostsCavalierAttack()
        {
            ResearchInstantly(0, "tech_animal_husbandry");
            ResearchInstantly(0, "tech_breeding");
            ResearchInstantly(0, "tech_cavalry");
            Assert.AreEqual(1.3f, _effects.GetUnitAttackMultiplier(0, UnitType.Cavalier), 0.01f);
            Assert.AreEqual(1f, _effects.GetUnitAttackMultiplier(0, UnitType.Pikeman), 0.01f);
        }

        [Test]
        public void Hygiene_GivesPopBonus()
        {
            ResearchInstantly(0, "tech_fishing");
            ResearchInstantly(0, "tech_preservation");
            ResearchInstantly(0, "tech_hygiene");
            Assert.AreEqual(2, _effects.GetHygieneBonus(0, BaseBuildingType.Residence));
            Assert.AreEqual(4, _effects.GetHygieneBonus(0, BaseBuildingType.NobleResidence));
            Assert.AreEqual(0, _effects.GetHygieneBonus(0, BaseBuildingType.Lodge));
        }

        [Test]
        public void Preservation_GivesFancyFoodBonus()
        {
            ResearchInstantly(0, "tech_fishing");
            ResearchInstantly(0, "tech_preservation");
            Assert.AreEqual(1, _effects.GetFancyFoodBonus(0));
        }

        [Test]
        public void OtherPlayer_NoBonus()
        {
            ResearchInstantly(0, "tech_plowing");
            Assert.AreEqual(1f, _effects.GetProductionMultiplier(1, "grain_barn"), 0.01f);
        }

        [Test]
        public void FortificationTech_DoublesFortBuildSpeed()
        {
            ResearchInstantly(0, "tech_masonry");
            ResearchInstantly(0, "tech_fortification_tech");
            Assert.AreEqual(2f, _effects.GetFortificationSpeedMultiplier(0), 0.01f);
        }

        private void ResearchInstantly(int playerId, string techId)
        {
            _research.StartResearch(playerId, techId);
            // Tick enough to complete
            _research.Tick(1000f);
        }
    }
}
