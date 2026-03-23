using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Data
{
    /// <summary>
    /// ScriptableObject defining a work yard type (Forester, Sawmill, etc.).
    /// Work yards attach to a parent building and produce goods.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWorkYard", menuName = "Settlers/WorkYard")]
    public class WorkYardDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string workYardId;
        public string displayName;

        [Header("Parent Building")]
        public BaseBuildingType parentBuildingType;

        [Header("Production")]
        public ProductionRecipe recipe;
        public ResourceNodeType requiredResourceNode;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefab;
    }
}
