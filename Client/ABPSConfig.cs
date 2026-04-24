using BepInEx.Configuration;
using System;

namespace acidphantasm_botplacementsystem
{
    internal static class ABPSConfig
    {
        private static int loadOrder = 200;
        
        private const string DespawnConfig = "1. Despawn Settings";
        public static ConfigEntry<bool> despawnFurthest;
        public static ConfigEntry<bool> despawnPmcs;
        public static ConfigEntry<float> despawnDistance;
        public static ConfigEntry<float> despawnTimer;
        
        private const string GeneralConfig = "2. General Settings";
        public static ConfigEntry<int> customsMapLimit;
        public static ConfigEntry<int> factoryMapLimit;
        public static ConfigEntry<int> interchangeMapLimit;
        public static ConfigEntry<int> labsMapLimit;
        public static ConfigEntry<int> lighthouseMapLimit;
        public static ConfigEntry<int> reserveMapLimit;
        public static ConfigEntry<int> groundZeroMapLimit;
        public static ConfigEntry<int> shorelineMapLimit;
        public static ConfigEntry<int> streetsMapLimit;
        public static ConfigEntry<int> woodsMapLimit;
        public static ConfigEntry<int> labyrinthMapLimit;
        
        private const string BossConfig = "3. Boss Settings";
        public static ConfigEntry<bool> regressiveChances;
        public static ConfigEntry<bool> progressiveChances;
        public static ConfigEntry<int> chanceStep;
        public static ConfigEntry<int> minimumChance;
        public static ConfigEntry<int> maximumChance;

        private const string PMCConfig = "4. PMC Settings";
        public static ConfigEntry<bool> pmcSpawnAnywhere;
        public static ConfigEntry<float> customs_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> factory_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> interchange_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> labs_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> lighthouse_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> reserve_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> groundZero_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> shoreline_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> streets_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> woods_PMCSpawnDistanceCheck;
        public static ConfigEntry<float> labyrinth_PMCSpawnDistanceCheck;

        private const string ScavConfig = "5. Scav Settings";
        public static ConfigEntry<int> softCap;
        public static ConfigEntry<int> pScavChance;
        public static ConfigEntry<bool> enableHotzones;
        public static ConfigEntry<int> zoneScavCap;
        public static ConfigEntry<int> hotzoneScavCap;
        public static ConfigEntry<int> hotzoneScavChance;
        public static ConfigEntry<float> customs_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> factory_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> interchange_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> labs_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> lighthouse_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> reserve_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> groundZero_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> shoreline_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> streets_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> woods_ScavSpawnDistanceCheck;
        public static ConfigEntry<float> labyrinth_ScavSpawnDistanceCheck;

        public static void InitABPSConfig(ConfigFile config)
        {
            // Despawn Settings
            despawnFurthest = config.Bind(
                DespawnConfig,
                "Enable Despawning",
                false,
                new ConfigDescription("Enabling this will only despawn scavs, if you want to also despawn PMCs you must also check the below option.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = loadOrder-- }));
            Plugin.DespawnFurthest = despawnFurthest.Value;
            despawnFurthest.SettingChanged += ABPS_SettingChanged;
            
            despawnPmcs = config.Bind(
                DespawnConfig,
                "Enable Despawning PMCs",
                false,
                new ConfigDescription("Allow ABPS to despawn PMCs. \nRequires `Enable Despawning`\n\n If you enable this and don't turn on PMC waves, then expect to have almost no PMCs in your raids. \nThat's on you.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = loadOrder-- }));
            Plugin.DespawnPmcs = despawnPmcs.Value;
            despawnPmcs.SettingChanged += ABPS_SettingChanged;
            
            despawnDistance = config.Bind(
                DespawnConfig,
                "Despawn Distance",
                250f,
                new ConfigDescription("Distance that bots must be from player to trigger despawning.",
                    new AcceptableValueRange<float>(100f, 500f),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = loadOrder-- }));
            Plugin.DespawnDistance = despawnDistance.Value;
            despawnDistance.SettingChanged += ABPS_SettingChanged;
            
            despawnTimer = config.Bind(
                DespawnConfig,
                "Despawn Timer",
                300f,
                new ConfigDescription("Timer for despawning, this is the MINIMUM time between despawning attempts. In Seconds.",
                    new AcceptableValueRange<float>(180f, 600f),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = loadOrder-- }));
            Plugin.DespawnTimer = despawnTimer.Value;
            despawnTimer.SettingChanged += ABPS_SettingChanged;
            
            // General Settings
            customsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Customs",
                23,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.CustomsMapLimit = customsMapLimit.Value;
            customsMapLimit.SettingChanged += ABPS_SettingChanged;

            factoryMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Factory",
                13,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.FactoryMapLimit = factoryMapLimit.Value;
            factoryMapLimit.SettingChanged += ABPS_SettingChanged;

            interchangeMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Interchange",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.InterchangeMapLimit = interchangeMapLimit.Value;
            interchangeMapLimit.SettingChanged += ABPS_SettingChanged;

            labsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Labs",
                19,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LabsMapLimit = labsMapLimit.Value;
            labsMapLimit.SettingChanged += ABPS_SettingChanged;

            lighthouseMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Lighthouse",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LighthouseMapLimit = lighthouseMapLimit.Value;
            lighthouseMapLimit.SettingChanged += ABPS_SettingChanged;

            reserveMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Reserve",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ReserveMapLimit = reserveMapLimit.Value;
            reserveMapLimit.SettingChanged += ABPS_SettingChanged;

            groundZeroMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Ground Zero",
                16,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.GroundZeroMapLimit = groundZeroMapLimit.Value;
            groundZeroMapLimit.SettingChanged += ABPS_SettingChanged;

            shorelineMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Shoreline",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ShorelineMapLimit = shorelineMapLimit.Value;
            shorelineMapLimit.SettingChanged += ABPS_SettingChanged;

            streetsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Streets",
                23,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.StreetsMapLimit = streetsMapLimit.Value;
            streetsMapLimit.SettingChanged += ABPS_SettingChanged;

            woodsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Woods",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.WoodsMapLimit = woodsMapLimit.Value;
            woodsMapLimit.SettingChanged += ABPS_SettingChanged;

            labyrinthMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Labyrinth",
                13,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LabyrinthMapLimit = labyrinthMapLimit.Value;
            labyrinthMapLimit.SettingChanged += ABPS_SettingChanged;

            
            // Boss stuff
            regressiveChances = config.Bind(
                BossConfig,
                "Regressive Boss Chances",
                false,
                new ConfigDescription("If a boss spawned in the previous raid, lower their chance by the Step Count.\nChanges do not take effect until next raid.",
                    null,
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.RegressiveChances = regressiveChances.Value;
            regressiveChances.SettingChanged += ABPS_SettingChanged;

            progressiveChances = config.Bind(
                BossConfig,
                "Progressive Boss Chances",
                false,
                new ConfigDescription("If a boss didn't spawn in the previous raid, raise their chance by the Step Count.\nChanges do not take effect until next raid.",
                null,
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ProgressiveChances = progressiveChances.Value;
            progressiveChances.SettingChanged += ABPS_SettingChanged;

            chanceStep = config.Bind(
                BossConfig,
                "Step Count",
                5,
                new ConfigDescription("Progressive: If a boss fails to spawn, how much to increase their spawn chance by.\n\nRegressive: The spawn chance is decreased by this amount if they spawned last raid.\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 15),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ChanceStep = chanceStep.Value;
            chanceStep.SettingChanged += ABPS_SettingChanged;

            minimumChance = config.Bind(
                BossConfig,
                "Minimum Chance",
                5,
                new ConfigDescription("If only Progressive Chances are enabled, a boss that spawns will reset to this value.\n\nIf Regressive Chances are enabled, spawn chance instead decays toward this value if they spawned in the previous raid.\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 25),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.MinimumChance = minimumChance.Value;
            minimumChance.SettingChanged += ABPS_SettingChanged;

            maximumChance = config.Bind(
                BossConfig,
                "Maximum Chance",
                100,
                new ConfigDescription("The highest value a boss's spawn chance can reach when progressive chances are enabled.\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(25, 100),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.MaximumChance = maximumChance.Value;
            maximumChance.SettingChanged += ABPS_SettingChanged;

            // PMC Settings
            pmcSpawnAnywhere = config.Bind(
                PMCConfig,
                "Allow PMC Spawn Anywhere",
                false,
                new ConfigDescription("Enable this if you want PMCs to spawn at any spawn point instead of Player Spawn points.\nNote that with this disabled, PMCs will still spawn anywhere if there are no player spawn points available.",
                    null,
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.PmcSpawnAnywhere = pmcSpawnAnywhere.Value;
            pmcSpawnAnywhere.SettingChanged += ABPS_SettingChanged;
            
            customs_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Customs", 
                100f, 
                new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.CustomsPmcSpawnDistanceCheck = customs_PMCSpawnDistanceCheck.Value;
            customs_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            factory_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Factory", 
                30f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.FactoryPmcSpawnDistanceCheck = factory_PMCSpawnDistanceCheck.Value;
            factory_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            interchange_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Interchange",
                125f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.InterchangePmcSpawnDistanceCheck = interchange_PMCSpawnDistanceCheck.Value;
            interchange_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labs_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Labs", 
                40f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LabsPmcSpawnDistanceCheck = labs_PMCSpawnDistanceCheck.Value;
            labs_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            lighthouse_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Lighthouse",
                125f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LighthousePmcSpawnDistanceCheck = lighthouse_PMCSpawnDistanceCheck.Value;
            lighthouse_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            reserve_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Reserve",
                90f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ReservePmcSpawnDistanceCheck = reserve_PMCSpawnDistanceCheck.Value;
            reserve_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;
            
            groundZero_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - GroundZero",
                85f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.GroundZeroPmcSpawnDistanceCheck = groundZero_PMCSpawnDistanceCheck.Value;
            groundZero_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            shoreline_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Shoreline", 
                130f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ShorelinePmcSpawnDistanceCheck = shoreline_PMCSpawnDistanceCheck.Value;
            shoreline_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            streets_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Streets", 
                120f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.StreetsPmcSpawnDistanceCheck = streets_PMCSpawnDistanceCheck.Value;
            streets_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            woods_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Woods",
                150f,
                new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.",
                new AcceptableValueRange<float>(10f, 175f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.WoodsPmcSpawnDistanceCheck = woods_PMCSpawnDistanceCheck.Value;
            woods_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labyrinth_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Labyrinth",
                20f,
                new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.",
                new AcceptableValueRange<float>(10f, 175f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LabyrinthPmcSpawnDistanceCheck = labyrinth_PMCSpawnDistanceCheck.Value;
            labyrinth_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;


            // Scav Settings

            softCap = config.Bind(
                ScavConfig, 
                "Scav Soft Cap", 
                3, 
                new ConfigDescription("How many open slots before hard cap to stop spawning additional scavs.\nEx..If 3, and map cap is 23 - will stop spawning scavs at 20 total.\nThis allows PMC waves if enabled to fill the remaining spots.", 
                new AcceptableValueRange<int>(0, 10), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.SoftCap = softCap.Value;
            softCap.SettingChanged += ABPS_SettingChanged;

            pScavChance = config.Bind(
                ScavConfig, 
                "PScav Chance", 
                20, 
                new ConfigDescription("How likely a scav spawning later in the raid is a Player Scav.",
                new AcceptableValueRange<int>(0, 100),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.PScavChance = pScavChance.Value;
            pScavChance.SettingChanged += ABPS_SettingChanged;

            zoneScavCap = config.Bind(
                ScavConfig,
                "Zone Cap",
                2,
                new ConfigDescription("How many scavs can spawn in any one zone (excluding Factory/Ground Zero).",
                new AcceptableValueRange<int>(0, 15),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ZoneScavCap = zoneScavCap.Value;
            zoneScavCap.SettingChanged += ABPS_SettingChanged;

            enableHotzones = config.Bind(
                ScavConfig,
                "Hotzones",
                false,
                new ConfigDescription("Enables hotzones around maps, more common or quest areas are considered hotzones.",
                null,
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.EnableHotzones = enableHotzones.Value;
            enableHotzones.SettingChanged += ABPS_SettingChanged;

            hotzoneScavCap = config.Bind(
                ScavConfig,
                "Hotzone Cap",
                4,
                new ConfigDescription("How many scavs can spawn in a hotzone, if you enable hotzones (excluding Factory/Ground Zero).",
                new AcceptableValueRange<int>(0, 15),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.HotzoneScavCap = hotzoneScavCap.Value;
            hotzoneScavCap.SettingChanged += ABPS_SettingChanged;

            hotzoneScavChance = config.Bind(
                ScavConfig,
                "Hotzone Chance",
                20,
                new ConfigDescription("How likely a scav is to spawn in a hotzone (excluding Factory/Ground Zero).",
                new AcceptableValueRange<int>(0, 100),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.HotzoneScavChance = hotzoneScavChance.Value;
            hotzoneScavChance.SettingChanged += ABPS_SettingChanged;

            customs_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig, 
                "Distance Limit - Customs", 
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(5f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.CustomsScavSpawnDistanceCheck = customs_ScavSpawnDistanceCheck.Value;
            customs_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            factory_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig, 
                "Distance Limit - Factory", 
                30f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(5f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.FactoryScavSpawnDistanceCheck = factory_ScavSpawnDistanceCheck.Value;
            factory_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            interchange_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Interchange",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.InterchangeScavSpawnDistanceCheck = interchange_ScavSpawnDistanceCheck.Value;
            interchange_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labs_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Labs", 
                40f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LabsScavSpawnDistanceCheck = labs_ScavSpawnDistanceCheck.Value;
            labs_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            lighthouse_ScavSpawnDistanceCheck = config.Bind
                (ScavConfig,
                "Distance Limit - Lighthouse",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LighthouseScavSpawnDistanceCheck = lighthouse_ScavSpawnDistanceCheck.Value;
            lighthouse_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            reserve_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Reserve",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ReserveScavSpawnDistanceCheck = reserve_ScavSpawnDistanceCheck.Value;
            reserve_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            groundZero_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - GroundZero",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.GroundZeroScavSpawnDistanceCheck = groundZero_ScavSpawnDistanceCheck.Value;
            groundZero_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            shoreline_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Shoreline",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.ShorelineScavSpawnDistanceCheck = shoreline_ScavSpawnDistanceCheck.Value;
            shoreline_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            streets_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Streets",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.StreetsScavSpawnDistanceCheck = streets_ScavSpawnDistanceCheck.Value;
            streets_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            woods_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Woods",
                45f,
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.WoodsScavSpawnDistanceCheck = woods_ScavSpawnDistanceCheck.Value;
            woods_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labyrinth_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Woods",
                45f,
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.LabyrinthScavSpawnDistanceCheck = labyrinth_ScavSpawnDistanceCheck.Value;
            labyrinth_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;
        }
        private static void ABPS_SettingChanged(object sender, EventArgs e)
        {
            Plugin.DespawnFurthest = despawnFurthest.Value;
            Plugin.DespawnDistance = despawnDistance.Value;
            
            Plugin.CustomsMapLimit = customsMapLimit.Value;
            Plugin.FactoryMapLimit = factoryMapLimit.Value;
            Plugin.InterchangeMapLimit = interchangeMapLimit.Value;
            Plugin.LabsMapLimit = labsMapLimit.Value;
            Plugin.LighthouseMapLimit = lighthouseMapLimit.Value;
            Plugin.ReserveMapLimit = reserveMapLimit.Value;
            Plugin.GroundZeroMapLimit = groundZeroMapLimit.Value;
            Plugin.ShorelineMapLimit = shorelineMapLimit.Value;
            Plugin.StreetsMapLimit = streetsMapLimit.Value;
            Plugin.WoodsMapLimit = woodsMapLimit.Value;
            Plugin.LabyrinthMapLimit = labyrinthMapLimit.Value;

            Plugin.RegressiveChances = regressiveChances.Value;
            Plugin.ProgressiveChances = progressiveChances.Value;
            Plugin.ChanceStep = chanceStep.Value;
            Plugin.MinimumChance = minimumChance.Value;
            Plugin.MaximumChance = maximumChance.Value;

            Plugin.PmcSpawnAnywhere = pmcSpawnAnywhere.Value;
            Plugin.CustomsPmcSpawnDistanceCheck = customs_PMCSpawnDistanceCheck.Value;
            Plugin.FactoryPmcSpawnDistanceCheck = factory_PMCSpawnDistanceCheck.Value;
            Plugin.InterchangePmcSpawnDistanceCheck = interchange_PMCSpawnDistanceCheck.Value;
            Plugin.LabsPmcSpawnDistanceCheck = labs_PMCSpawnDistanceCheck.Value;
            Plugin.LighthousePmcSpawnDistanceCheck = lighthouse_PMCSpawnDistanceCheck.Value;
            Plugin.ReservePmcSpawnDistanceCheck = reserve_PMCSpawnDistanceCheck.Value;
            Plugin.GroundZeroPmcSpawnDistanceCheck = groundZero_PMCSpawnDistanceCheck.Value;
            Plugin.ShorelinePmcSpawnDistanceCheck = shoreline_PMCSpawnDistanceCheck.Value;
            Plugin.StreetsPmcSpawnDistanceCheck = streets_PMCSpawnDistanceCheck.Value;
            Plugin.WoodsPmcSpawnDistanceCheck = woods_PMCSpawnDistanceCheck.Value;
            Plugin.LabyrinthPmcSpawnDistanceCheck = labyrinth_PMCSpawnDistanceCheck.Value;

            Plugin.SoftCap = softCap.Value;
            Plugin.PScavChance = pScavChance.Value;
            Plugin.EnableHotzones = enableHotzones.Value;
            Plugin.ZoneScavCap = zoneScavCap.Value;
            Plugin.HotzoneScavCap = hotzoneScavCap.Value;
            Plugin.CustomsScavSpawnDistanceCheck = customs_ScavSpawnDistanceCheck.Value;
            Plugin.FactoryScavSpawnDistanceCheck = factory_ScavSpawnDistanceCheck.Value;
            Plugin.InterchangeScavSpawnDistanceCheck = interchange_ScavSpawnDistanceCheck.Value;
            Plugin.LabsScavSpawnDistanceCheck = labs_ScavSpawnDistanceCheck.Value;
            Plugin.LighthouseScavSpawnDistanceCheck = lighthouse_ScavSpawnDistanceCheck.Value;
            Plugin.ReserveScavSpawnDistanceCheck = reserve_ScavSpawnDistanceCheck.Value;
            Plugin.GroundZeroScavSpawnDistanceCheck = groundZero_ScavSpawnDistanceCheck.Value;
            Plugin.ShorelineScavSpawnDistanceCheck = shoreline_ScavSpawnDistanceCheck.Value;
            Plugin.StreetsScavSpawnDistanceCheck = streets_ScavSpawnDistanceCheck.Value;
            Plugin.WoodsScavSpawnDistanceCheck = woods_ScavSpawnDistanceCheck.Value;
            Plugin.LabyrinthScavSpawnDistanceCheck = labyrinth_ScavSpawnDistanceCheck.Value;
        }
    }
}
