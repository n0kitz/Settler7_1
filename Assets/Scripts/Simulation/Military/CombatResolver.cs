namespace Settlers.Simulation
{
    /// <summary>
    /// Auto-resolves combat when an army arrives at an enemy/neutral sector.
    /// Combat is deterministic: compare total attack vs total defense.
    /// Fortified sectors require breach-capable units (Musketeer/Cannon).
    /// Pure C# — no UnityEngine references.
    /// </summary>
    public class CombatResolver
    {
        private readonly SectorGraph _graph;
        private readonly EventBus _eventBus;
        private TechEffects _techEffects;

        public CombatResolver(SectorGraph graph, EventBus eventBus)
        {
            _graph = graph;
            _eventBus = eventBus;
        }

        /// <summary>Set the TechEffects reference for unit attack bonuses.</summary>
        public void SetTechEffects(TechEffects techEffects) => _techEffects = techEffects;

        /// <summary>
        /// Attempt to conquer a sector with an army. Returns the result.
        /// </summary>
        public CombatResult ResolveCombat(General attacker, int targetSectorId)
        {
            var sector = _graph.GetSector(targetSectorId);
            if (sector.OwnerId == attacker.OwnerId)
                return new CombatResult(false, 0, 0); // Can't attack own sector

            // Fortified sectors require breach-capable units
            if (sector.IsFortified && !attacker.CanBreachFortification())
                return new CombatResult(false, 0, 0);

            int attackPower = GetTechBoostedAttack(attacker);
            int defensePower = CalculateDefensePower(sector);

            // Fortification bonus: +50% defense
            if (sector.IsFortified)
                defensePower = (int)(defensePower * 1.5f);

            bool victory = attackPower > defensePower;

            // Calculate losses (simplified: proportional to power ratio)
            int attackerLosses = 0;
            int defenderLosses = 0;

            if (victory)
            {
                // Attacker wins: loses soldiers proportional to defense/attack
                float lossRatio = (float)defensePower / System.Math.Max(1, attackPower);
                attackerLosses = System.Math.Max(1,
                    (int)(attacker.TotalSoldiers * lossRatio * 0.5f));
                defenderLosses = sector.GarrisonStrength;

                // Apply attacker losses (remove weakest units first)
                ApplyLosses(attacker, attackerLosses);

                // Conquer the sector
                int prevOwner = sector.OwnerId;
                sector.SetOwner(attacker.OwnerId);
                sector.SetGarrison(0);
                if (sector.IsFortified)
                    sector.SetFortified(false); // Fortifications destroyed on conquest

                _eventBus.Publish(new SectorConqueredEvent(
                    targetSectorId, attacker.OwnerId, prevOwner, ConquestMethod.Military));
                _eventBus.Publish(new CombatResolvedEvent(
                    attacker.OwnerId, targetSectorId, true, attackerLosses, defenderLosses));
            }
            else
            {
                // Attacker loses: heavier losses
                float lossRatio = (float)attackPower / System.Math.Max(1, defensePower);
                attackerLosses = System.Math.Max(1,
                    (int)(attacker.TotalSoldiers * (1f - lossRatio * 0.3f)));
                defenderLosses = System.Math.Max(0,
                    (int)(sector.GarrisonStrength * lossRatio * 0.3f));

                ApplyLosses(attacker, attackerLosses);
                sector.SetGarrison(System.Math.Max(0,
                    sector.GarrisonStrength - defenderLosses));

                _eventBus.Publish(new CombatResolvedEvent(
                    attacker.OwnerId, targetSectorId, false, attackerLosses, defenderLosses));
            }

            return new CombatResult(victory, attackerLosses, defenderLosses);
        }

        private int GetTechBoostedAttack(General attacker)
        {
            if (_techEffects == null) return attacker.TotalAttack;

            int total = 0;
            bool hasStandardBearer = attacker.GetUnitCount(UnitType.StandardBearer) > 0;
            foreach (var kvp in attacker.Units)
            {
                float techMult = _techEffects.GetUnitAttackMultiplier(attacker.OwnerId, kvp.Key);
                total += (int)(UnitStats.GetAttack(kvp.Key) * kvp.Value * techMult);
            }
            if (hasStandardBearer) total = (int)(total * 1.15f);
            return total;
        }

        private int CalculateDefensePower(Sector sector)
        {
            // Neutral/enemy garrison: garrison strength * base defense value
            return sector.GarrisonStrength * 4;
        }

        private void ApplyLosses(General general, int losses)
        {
            int remaining = losses;
            // Remove pikemen first, then musketeers, then others
            var order = new[]
            {
                UnitType.Pikeman, UnitType.Musketeer,
                UnitType.StandardBearer, UnitType.Cavalier, UnitType.Cannon
            };

            foreach (var unitType in order)
            {
                while (remaining > 0 && general.GetUnitCount(unitType) > 0)
                {
                    general.RemoveUnit(unitType);
                    remaining--;
                }
            }
        }
    }

    public readonly struct CombatResult
    {
        public readonly bool Victory;
        public readonly int AttackerLosses;
        public readonly int DefenderLosses;

        public CombatResult(bool victory, int attackerLosses, int defenderLosses)
        {
            Victory = victory;
            AttackerLosses = attackerLosses;
            DefenderLosses = defenderLosses;
        }
    }

    public readonly struct CombatResolvedEvent
    {
        public readonly int AttackerId;
        public readonly int SectorId;
        public readonly bool Victory;
        public readonly int AttackerLosses;
        public readonly int DefenderLosses;

        public CombatResolvedEvent(int attackerId, int sectorId, bool victory,
            int attackerLosses, int defenderLosses)
        {
            AttackerId = attackerId;
            SectorId = sectorId;
            Victory = victory;
            AttackerLosses = attackerLosses;
            DefenderLosses = defenderLosses;
        }
    }
}
