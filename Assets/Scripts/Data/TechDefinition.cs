using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Data
{
    /// <summary>
    /// ScriptableObject defining a technology in the tech tree.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTech", menuName = "Settlers/Technology")]
    public class TechDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string techId;
        public string displayName;
        [TextArea] public string description;

        [Header("Research")]
        public TechTree.TechTier tier;
        public float researchTime;
        public TechDefinition prerequisite;

        [Header("Visuals")]
        public Sprite icon;
    }
}
