using acidphantasm_botplacementsystem.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT;

namespace acidphantasm_botplacementsystem
{
    [BepInPlugin("com.acidphantasm.botplacementsystem", "acidphantasm-botplacementsystem", "2.0.10")]
    [BepInDependency("com.fika.headless", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        public static bool despawnFurthest;
        public static bool despawnPmcs;
        public static float despawnDistance;
        public static float despawnTimer;
        
        public static int customsMapLimit;
        public static int factoryMapLimit;
        public static int interchangeMapLimit;
        public static int labsMapLimit;
        public static int lighthouseMapLimit;
        public static int reserveMapLimit;
        public static int groundZeroMapLimit;
        public static int shorelineMapLimit;
        public static int streetsMapLimit;
        public static int woodsMapLimit;
        public static int labyrinthMapLimit;
        
        public static bool regressiveChances;
        public static bool progressiveChances;
        public static int chanceStep;
        public static int minimumChance;
        public static int maximumChance;

        public static bool pmcSpawnAnywhere;
        public static float customs_PMCSpawnDistanceCheck;
        public static float factory_PMCSpawnDistanceCheck;
        public static float interchange_PMCSpawnDistanceCheck;
        public static float labs_PMCSpawnDistanceCheck;
        public static float lighthouse_PMCSpawnDistanceCheck;
        public static float reserve_PMCSpawnDistanceCheck;
        public static float groundZero_PMCSpawnDistanceCheck;
        public static float shoreline_PMCSpawnDistanceCheck;
        public static float streets_PMCSpawnDistanceCheck;
        public static float woods_PMCSpawnDistanceCheck;
        public static float labyrinth_PMCSpawnDistanceCheck;


        public static int softCap;
        public static int pScavChance;
        public static int zoneScavCap;
        public static bool enableHotzones;
        public static int hotzoneScavCap;
        public static int hotzoneScavChance;
        public static float customs_ScavSpawnDistanceCheck;
        public static float factory_ScavSpawnDistanceCheck;
        public static float interchange_ScavSpawnDistanceCheck;
        public static float labs_ScavSpawnDistanceCheck;
        public static float lighthouse_ScavSpawnDistanceCheck;
        public static float reserve_ScavSpawnDistanceCheck;
        public static float groundZero_ScavSpawnDistanceCheck;
        public static float shoreline_ScavSpawnDistanceCheck;
        public static float streets_ScavSpawnDistanceCheck;
        public static float woods_ScavSpawnDistanceCheck;
        public static float labyrinth_ScavSpawnDistanceCheck;

        public static BotSpawner botSpawnerInstance;

        internal void Awake()
        {
            LogSource = Logger;

            /*
             * This patch is only for development purposes in specific scenarios (or it would be in IFDEBUG)
            */
            //new OnGameStartedPatch().Enable();

            // Trigger /apbs/save
            new UnregisterPlayerPatch().Enable();

            // Trigger /apbs/load
            new MenuLoadPatch().Enable();

            // Set bot limits
            new MaxBotLimitPatch().Enable();

            // Progressive Chances patches
            new LocalGameProgressivePatch().Enable();
            new BossAddProgressionPatch().Enable();

            // Patch to build new lists for everything
            new PMCWaveCountPatch().Enable();

            // Main PMC Method Patch to trigger ABPS spawning instead
            new PMCDistancePatch().Enable();

            // If assaultgroup, make assault instead. This stops the "wave" of scavs that spawn in NewSpawn mode that track and rush the player
            new AssaultGroupPatch().Enable();

            // Patch the NewSpawn primary method (it's in Update) to spawn scavs differently
            new NonWavesSpawnScenarioUpdatePatch().Enable();

            // Zone Reselector for Scavs - primarily for redistribution based on active scavs in a zone and hotzone configuration
            new TryToSpawnInZonePatch().Enable();
            
            // Check enemy patch to prevent bots in the same group being enemies, but allow other groups containing the same PMC type to be enemies
            new IsPlayerEnemyPatch().Enable();
            
            ABPSConfig.InitABPSConfig(Config);
        }
    }
}
