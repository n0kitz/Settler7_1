using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Data
{
    /// <summary>
    /// ScriptableObject defining a base building type (Lodge, Farm, etc.).
    /// Contains costs, population, and allowed work yard types.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuilding", menuName = "Settlers/Building")]
    public class BuildingDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string buildingId;
        public string displayName;
        public BaseBuildingType type;

        [Header("Costs")]
        public int plankCost;
        public int stoneCost;

        [Header("Population")]
        public int basePopulation;
        public int upgradePopulationBonus;
        public int maxUpgradeLevel;

        [Header("Properties")]
        public bool requiresFoodToFunction;
        public int maxWorkYards = 3;

        [Header("Work Yards")]
        public WorkYardDefinition[] allowedWorkYards;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefab;
        public Color buildingColor = Color.white;

        /// <summary>Total population at max upgrade level.</summary>
        public int MaxPopulation => basePopulation + (upgradePopulationBonus * maxUpgradeLevel);
    }
}
