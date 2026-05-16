using System;

namespace Settlers.Simulation
{
    /// <summary>
    /// Tracks tutorial progress and checks completion conditions.
    /// Lives in the Simulation layer so it can subscribe to EventBus.
    /// Fires C# events consumed by the presentation/UI layer.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class TutorialSystem
    {
        // --- C# events for presentation layer ---

        /// <summary>Fired when a new step becomes active.</summary>
        public event Action<TutorialStep> OnStepStarted;

        /// <summary>Fired when the last step is completed.</summary>
        public event Action OnTutorialComplete;

        /// <summary>Fired when the player clicks Skip.</summary>
        public event Action OnTutorialSkipped;

        // --- State ---

        private readonly TutorialStep[] _steps;
        private readonly EventBus _events;
        private int _stepIndex;
        private bool _isActive;
        private bool _subscribed;

        public TutorialStep CurrentStep => _stepIndex < _steps.Length ? _steps[_stepIndex] : null;
        public bool IsComplete => _stepIndex >= _steps.Length;
        public bool IsActive => _isActive;
        public int CurrentStepIndex => _stepIndex;
        public int TotalSteps => _steps.Length;

        public TutorialSystem(EventBus events)
        {
            _events = events;
            _steps = BuildDefaultSteps();
        }

        /// <summary>Begin the tutorial from step 0.</summary>
        public void Activate()
        {
            _isActive = true;
            _stepIndex = 0;
            SubscribeEvents();
            OnStepStarted?.Invoke(CurrentStep);
        }

        /// <summary>Manually advance to the next step (used by Next button).</summary>
        public void Advance()
        {
            if (!_isActive || IsComplete) return;
            _stepIndex++;
            if (IsComplete)
            {
                _isActive = false;
                OnTutorialComplete?.Invoke();
            }
            else
            {
                OnStepStarted?.Invoke(CurrentStep);
            }
        }

        /// <summary>Skip the tutorial entirely.</summary>
        public void Skip()
        {
            if (!_isActive) return;
            _isActive = false;
            OnTutorialSkipped?.Invoke();
        }

        // --- Event handling ---

        private void SubscribeEvents()
        {
            if (_subscribed) return;
            _subscribed = true;
            _events.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            _events.Subscribe<BuildingCompletedEvent>(OnBuildingCompleted);
            _events.Subscribe<SectorConqueredEvent>(OnSectorConquered);
            _events.Subscribe<ResourceChangedEvent>(OnResourceChanged);
        }

        private void OnBuildingPlaced(BuildingPlacedEvent e)
        {
            var s = CurrentStep;
            if (s == null || s.Condition != TutorialConditionType.PlaceBuilding) return;
            bool typeMatch = s.ConditionParam == null
                || s.ConditionParam == e.BuildingType.ToString();
            if (typeMatch && s.AutoAdvance) Advance();
        }

        private void OnBuildingCompleted(BuildingCompletedEvent e)
        {
            var s = CurrentStep;
            if (s == null || s.Condition != TutorialConditionType.BuildComplete) return;
            if (s.AutoAdvance) Advance();
        }

        private void OnSectorConquered(SectorConqueredEvent e)
        {
            var s = CurrentStep;
            if (s == null || s.Condition != TutorialConditionType.ConquerSector) return;
            if (e.NewOwnerId == 0 && s.AutoAdvance) Advance();
        }

        private void OnResourceChanged(ResourceChangedEvent e)
        {
            var s = CurrentStep;
            if (s == null || s.Condition != TutorialConditionType.ResourceReached) return;
            if (e.PlayerId != 0) return;
            if (s.ConditionParam != null
                && s.ConditionParam == e.Type.ToString()
                && e.NewAmount >= s.ConditionAmount
                && s.AutoAdvance)
                Advance();
        }

        // --- Default tutorial steps ---

        private static TutorialStep[] BuildDefaultSteps() => new[]
        {
            new TutorialStep(
                "Welcome to Die Siedler!",
                "Build a thriving economy and earn Victory Points to win.\n\n" +
                "Start by placing a Lodge — open the Build Menu (press B) and click a sector you own.",
                TutorialConditionType.PlaceBuilding, conditionParam: null,
                highlightTarget: "Btn_BuildMenu"),

            new TutorialStep(
                "Production Chains",
                "Each building provides work yards. Every work yard needs " +
                "one Settler + one Tool to operate.\n\n" +
                "All goods flow automatically to your Storehouse via Carriers.",
                TutorialConditionType.None, autoAdvance: false),

            new TutorialStep(
                "Grow Your Economy",
                "Build a Farm to grow crops. Food is the most powerful lever:\n" +
                "• Plain food (Bread, Fish) → ×2 production speed\n" +
                "• Fancy food (Sausages + Beer) → ×3 speed\n\n" +
                "Build a Farm now.",
                TutorialConditionType.PlaceBuilding, conditionParam: "Farm",
                highlightTarget: "Btn_BuildMenu"),

            new TutorialStep(
                "Expand Your Territory",
                "Neutral sectors contain iron, gold, and coal you need.\n\n" +
                "Conquer them three ways:\n" +
                "• Military: send a General with an army\n" +
                "• Proselytism: send 6+ Clerics\n" +
                "• Bribery: spend Coins + Garments\n\n" +
                "Click a neutral sector and choose a conquest method.",
                TutorialConditionType.ConquerSector,
                highlightTarget: "SectorPanel"),

            new TutorialStep(
                "Technology Research",
                "The Church building unlocks Clerics who research technologies " +
                "at Monasteries. 18 techs across 3 tiers give powerful bonuses.\n\n" +
                "Each tech can only be researched by ONE player — first come, first served!\n\n" +
                "Press T to open the Tech Tree.",
                TutorialConditionType.None, autoAdvance: false,
                highlightTarget: "TechTreeUI"),

            new TutorialStep(
                "Victory Points",
                "Earn VPs by controlling sectors, training armies, claiming trade " +
                "outposts, and more. There are 10 dynamic VPs and several permanent ones.\n\n" +
                "The first player to hold enough VPs for 3 minutes wins!\n\n" +
                "Check the VP tracker in the top-right corner of the HUD.",
                TutorialConditionType.None, autoAdvance: false,
                highlightTarget: "VictoryPanel"),

            new TutorialStep(
                "You're Ready!",
                "You know the basics of Die Siedler 7. Return to the Main Menu " +
                "and start a real skirmish against AI opponents.\n\n" +
                "Good luck, my lord!",
                TutorialConditionType.None, autoAdvance: false),
        };
    }
}
