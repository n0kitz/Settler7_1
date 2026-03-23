using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Data
{
    /// <summary>
    /// ScriptableObject defining a production recipe.
    /// Maps inputs to outputs with a cycle duration.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Settlers/Recipe")]
    public class ProductionRecipe : ScriptableObject
    {
        [Header("Identity")]
        public string recipeId;
        public string displayName;

        [Header("Inputs / Outputs")]
        public ResourceAmount[] inputs;
        public ResourceAmount[] outputs;

        [Header("Timing")]
        public float cycleDuration = 10f;

        [Header("Requirements")]
        public ResourceNodeType requiredResourceNode;
    }

    /// <summary>
    /// A type + amount pair for recipe inputs/outputs.
    /// </summary>
    [System.Serializable]
    public struct ResourceAmount
    {
        public ResourceType type;
        public int amount;

        public ResourceAmount(ResourceType type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
}
