using UnityEngine;
using Settlers.Simulation;

namespace Settlers.Data
{
    /// <summary>
    /// ScriptableObject defining a prestige unlock node.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPrestigeUnlock", menuName = "Settlers/Prestige Unlock")]
    public class PrestigeUnlockDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string unlockId;
        public string displayName;
        [TextArea] public string description;

        [Header("Tree Position")]
        public PrestigeDatabase.PrestigeBranch branch;
        public int minLevel;
        public PrestigeUnlockDefinition prerequisite;

        [Header("Visuals")]
        public Sprite icon;
    }
}
