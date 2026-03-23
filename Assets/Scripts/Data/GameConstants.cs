using UnityEngine;

namespace Settlers.Data
{
    /// <summary>
    /// Single source of truth for all magic numbers.
    /// Create one instance via Assets > Create > Settlers > GameConstants.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConstants", menuName = "Settlers/GameConstants")]
    public class GameConstants : ScriptableObject
    {
        [Header("Core")]
        public int prestigePointsPerLevel = 5;
        public int maxWorkYardsPerBuilding = 3;
        public int carrierMaxItems = 3;
        public float victoryCountdownSeconds = 180f;

        [Header("Military")]
        public int maxSoldiersPerGeneral = 35;
        public int maxGenerals = 5;

        [Header("Population")]
        public int residenceBasePop = 4;
        public int residenceUpgradePop = 4;
        public int nobleResidenceBasePop = 5;
        public int nobleResidenceUpgradePop = 5;
        public int hygieneResidenceBonus = 2;
        public int hygieneNobleBonus = 4;

        [Header("Food Boosting")]
        public int plainFoodMultiplier = 2;
        public int fancyFoodMultiplier = 3;
        public int nobleResidencePlainMultiplier = 1;
        public int nobleResidenceFancyMultiplier = 2;

        [Header("VP Thresholds")]
        public int vpFieldMarshalMin = 20;
        public int vpMetropolisMin = 25;
        public int vpEmperorMin = 3;
        public int vpBankerMin = 25;
        public int vpSunKingMin = 5;
        public int vpTradingCompanyMin = 5;
        public int vpFountainMin = 3;
        public float vpPacifistMinSeconds = 600f;
        public int vpEconomistMinPercent = 75;
        public int vpGeneralissimoMin = 20;

        [Header("Camera")]
        public float cameraPanSpeed = 20f;
        public float cameraZoomSpeed = 10f;
        public float cameraRotateSpeed = 90f;
        public float cameraMinDistance = 15f;
        public float cameraMaxDistance = 200f;
        public float cameraMinElevation = 0.5f;
        public float cameraMaxElevation = 1.3f;
        public float cameraSectorOverviewThreshold = 120f;

        [Header("Construction")]
        public float constructionBaseTime = 10f;
        public int constructorBaseCost = 1;
    }
}
