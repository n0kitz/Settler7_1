namespace Settlers.Simulation
{
    /// <summary>
    /// A single player-initiated action captured during a match.
    /// ActionType is a string tag (e.g., "PlaceBuilding", "MoveArmy") so
    /// new action types can be added without breaking deserialization of old replays.
    /// Payload is a semicolon-delimited param list specific to each ActionType.
    /// </summary>
    public sealed class ActionRecord
    {
        public readonly float  Timestamp;   // seconds since match start
        public readonly int    PlayerId;
        public readonly string ActionType;
        public readonly string Payload;     // e.g., "sectorId=3;buildingType=Lodge"

        public ActionRecord(float timestamp, int playerId, string actionType, string payload = "")
        {
            Timestamp  = timestamp;
            PlayerId   = playerId;
            ActionType = actionType ?? "";
            Payload    = payload ?? "";
        }

        // --- Known ActionType constants ---

        public const string PLACE_BUILDING   = "PlaceBuilding";
        public const string RESEARCH_TECH    = "ResearchTech";
        public const string MOVE_ARMY        = "MoveArmy";
        public const string HIRE_GENERAL     = "HireGeneral";
        public const string ACCEPT_QUEST     = "AcceptQuest";
        public const string CLAIM_OUTPOST    = "ClaimOutpost";
        public const string CONQUER_SECTOR   = "ConquerSector";
        public const string FORTIFY_SECTOR   = "FortifySector";
        public const string TOGGLE_FOOD_BOOST = "ToggleFoodBoost";

        public override string ToString() =>
            $"[{Timestamp:F2}s] P{PlayerId} {ActionType} {Payload}";
    }
}
