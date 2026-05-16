using System;

namespace Settlers.Simulation
{
    /// <summary>The type of condition required to advance past a tutorial step.</summary>
    public enum TutorialConditionType
    {
        None,            // Player clicks Next manually
        PlaceBuilding,   // Place any building; ConditionParam = type name or null for any
        ConquerSector,   // Player 0 conquers any sector
        ResourceReached, // ConditionParam = ResourceType name, ConditionAmount = minimum
        BuildComplete,   // Any building finishes construction
    }

    /// <summary>
    /// A single step in the tutorial sequence.
    /// Immutable data — created once at startup.
    /// </summary>
    public sealed class TutorialStep
    {
        /// <summary>Short heading shown in the bubble.</summary>
        public readonly string Title;

        /// <summary>Body text explaining what to do.</summary>
        public readonly string Message;

        /// <summary>How the step is completed.</summary>
        public readonly TutorialConditionType Condition;

        /// <summary>
        /// Condition-specific parameter.
        /// PlaceBuilding → BaseBuildingType name (e.g. "Lodge"), or null for any.
        /// ResourceReached → ResourceType name.
        /// </summary>
        public readonly string ConditionParam;

        /// <summary>Minimum amount for ResourceReached condition.</summary>
        public readonly int ConditionAmount;

        /// <summary>Name of the UI element to highlight (matches GameObject name).</summary>
        public readonly string HighlightTarget;

        /// <summary>If true the step auto-advances when the condition is met.</summary>
        public readonly bool AutoAdvance;

        public TutorialStep(string title, string message,
            TutorialConditionType condition = TutorialConditionType.None,
            string conditionParam = null, int conditionAmount = 0,
            string highlightTarget = null, bool autoAdvance = true)
        {
            Title = title;
            Message = message;
            Condition = condition;
            ConditionParam = conditionParam;
            ConditionAmount = conditionAmount;
            HighlightTarget = highlightTarget;
            AutoAdvance = autoAdvance;
        }
    }
}
