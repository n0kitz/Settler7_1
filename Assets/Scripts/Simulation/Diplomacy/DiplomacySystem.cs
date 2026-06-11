using System.Collections.Generic;

namespace Settlers.Simulation
{
    /// <summary>
    /// Manages pairwise diplomatic relations between all players.
    /// Processes actions from human or AI players, publishes status change events.
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class DiplomacySystem
    {
        private const int GIFT_COIN_AMOUNT = 50;

        private readonly GameState _state;
        private readonly Dictionary<(int, int), DiplomaticStatus> _relations =
            new Dictionary<(int, int), DiplomaticStatus>();

        public DiplomacySystem(GameState state)
        {
            _state = state;
            // All relations start as Peace
            int n = state.PlayerCount;
            for (int a = 0; a < n; a++)
                for (int b = a + 1; b < n; b++)
                    _relations[(a, b)] = DiplomaticStatus.Peace;
        }

        /// <summary>Get diplomatic status between two players (order-independent).</summary>
        public DiplomaticStatus GetStatus(int a, int b)
        {
            var key = MakeKey(a, b);
            return _relations.TryGetValue(key, out var s) ? s : DiplomaticStatus.Peace;
        }

        /// <summary>True if the attacker is allowed to assault the defender militarily.</summary>
        public bool CanAttack(int attackerId, int defenderId)
            => GetStatus(attackerId, defenderId).AllowsAttack();

        /// <summary>
        /// Process a diplomatic action. Returns true if accepted/successful.
        /// For AI targets, uses AIDiplomacyDecider. DeclareWar always succeeds.
        /// </summary>
        public bool ProcessAction(DiplomaticAction action)
        {
            int from = action.FromPlayerId;
            int to   = action.ToPlayerId;

            if (action.Type == DiplomaticActionType.DeclareWar)
            {
                SetStatus(from, to, DiplomaticStatus.War);
                return true;
            }

            bool accepted = ResolveAcceptance(action);
            if (!accepted) return false;

            ApplyAccepted(action);
            return true;
        }

        private bool ResolveAcceptance(DiplomaticAction action)
        {
            int to = action.ToPlayerId;
            // Player 0 = human; always auto-accepts via UI
            if (to == 0) return true;

            AIController ai = null;
            foreach (var controller in _state.AIPlayers)
                if (controller.PlayerId == to) { ai = controller; break; }
            if (ai == null) return false;

            int power = AIDiplomacyDecider.EstimatePowerBalance(_state, to, action.FromPlayerId);
            return AIDiplomacyDecider.ShouldAccept(action, ai.Profile.Personality, power);
        }

        private void ApplyAccepted(DiplomaticAction action)
        {
            int from = action.FromPlayerId;
            int to   = action.ToPlayerId;

            switch (action.Type)
            {
                case DiplomaticActionType.ProposeNonAggression:
                    SetStatus(from, to, DiplomaticStatus.NonAggression);
                    break;
                case DiplomaticActionType.ProposeAlliance:
                    SetStatus(from, to, DiplomaticStatus.Alliance);
                    break;
                case DiplomaticActionType.OfferGift:
                    TransferCoins(from, to, GIFT_COIN_AMOUNT);
                    break;
                case DiplomaticActionType.DemandTribute:
                    TransferCoins(to, from, GIFT_COIN_AMOUNT / 2);
                    break;
            }
        }

        private void SetStatus(int a, int b, DiplomaticStatus newStatus)
        {
            var key = MakeKey(a, b);
            var old = _relations.TryGetValue(key, out var s) ? s : DiplomaticStatus.Peace;
            if (old == newStatus) return;
            _relations[key] = newStatus;
            _state.Events.Publish(new DiplomaticStatusChangedEvent(a, b, old, newStatus));
        }

        private void TransferCoins(int fromPlayer, int toPlayer, int amount)
        {
            if (!_state.PlayerResources.TryGetValue(fromPlayer, out var fromRes)) return;
            if (!_state.PlayerResources.TryGetValue(toPlayer,   out var toRes))   return;
            int actual = System.Math.Min(amount, fromRes.Get(ResourceType.Coins));
            if (actual <= 0) return;
            fromRes.TrySpend(ResourceType.Coins, actual);
            toRes.Add(ResourceType.Coins, actual);
        }

        private static (int, int) MakeKey(int a, int b)
            => a < b ? (a, b) : (b, a);
    }
}
