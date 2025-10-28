using acidphantasm_botplacementsystem.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;

namespace acidphantasm_botplacementsystem
{
    [BepInPlugin("com.acidphantasm.botplacementsystem", "acidphantasm-botplacementsystem", "2.0.1")]
    [BepInDependency("com.fika.headless", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        

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
        public static bool progressiveChances;
        public static int chanceStep;
        public static int minimumChance;
        public static int maximumChance;

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

        internal void Awake()
        {
            LogSource = Logger;

            new MaxBotLimitPatch().Enable();

            //new OnGameStartedPatch().Enable();
            new MenuLoadPatch().Enable();

            new LocalGameProgressivePatch().Enable();
            new BossAddProgressionPatch().Enable();
            new PMCWaveCountPatch().Enable();
            new PMCDistancePatch().Enable();
            new AssaultGroupPatch().Enable();
            new NonWavesSpawnScenarioUpdatePatch().Enable();
            new TryToSpawnInZonePatch().Enable();
            
            ABPSConfig.InitABPSConfig(Config);
        }
    }
}
