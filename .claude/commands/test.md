Run the NUnit test suite and report results.

## How to run tests

Unity NUnit tests require the Unity Editor — they cannot be run from the command line without Unity.

### In Unity Editor (primary method)
1. Open Unity Editor with this project
2. Go to **Window > General > Test Runner**
3. Switch to **Edit Mode** tab (simulation tests don't need Play Mode)
4. Click **Run All** or select specific test classes
5. Tests are in `Assets/Tests/Editor/` (23 test files)

### What each test file covers
| File | System tested |
|------|---------------|
| AITests.cs | AIController decision-making |
| BuildingAndWorkYardTests.cs | Building construction, work yard assignment |
| ConquestRewardTests.cs | Sector conquest reward packages |
| ConstructionTests.cs | ConstructionSystem progress, completion |
| FoodBoostTests.cs | FoodBoostCalculator multipliers |
| FortificationTests.cs | FortificationSystem sector hardening |
| InputReservationTests.cs | Work yard input reservation (no mid-cycle theft) |
| LargeMapTests.cs | MapFactory large map generation |
| LogisticsTests.cs | LogisticsSystem carrier dispatch, delivery |
| MapFactoryTests.cs | MapFactory all map types |
| MilitaryTests.cs | ArmySystem training, movement, CombatResolver |
| PrestigeTests.cs | PrestigeSystem points, levels, unlocks |
| ProductionFoodAndReservationTests.cs | Production + food + reservation integration |
| ProductionTests.cs | ProductionSystem recipe cycles, output |
| QuestTests.cs | QuestSystem trigger conditions, completion |
| SaveLoadTests.cs | SaveSystem serialization round-trip |
| SectorGraphTests.cs | SectorGraph BFS pathfinding, adjacency |
| TechEffectsIntegrationTests.cs | TechEffects applied to production |
| TechEffectsTests.cs | TechEffects individual bonuses |
| TechnologyTests.cs | ResearchSystem first-come-first-served |
| TradeTests.cs | TradeSystem outpost claiming, trader movement |
| UpgradeTests.cs | UpgradeSystem building tier upgrades |
| VictoryTests.cs | VictorySystem VP counting, countdown |

### Quick validation without Unity
Run architecture checks instead:
```
/validate
```
This checks layer violations, file sizes, and file counts without needing Unity.

### CI
GitHub Actions runs code quality checks (layer violations, file sizes) on every push via `.github/workflows/ci.yml`. Full NUnit runs require Unity and must be done manually.
