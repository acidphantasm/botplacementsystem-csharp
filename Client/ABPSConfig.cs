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
        public static ConfigEntry<bool> progressiveChances;
        public static ConfigEntry<int> chanceStep;
        public static ConfigEntry<int> minimumChance;
        public static ConfigEntry<int> maximumChance;

        private const string PMCConfig = "3. PMC Settings";
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

        private const string ScavConfig = "4. Scav Settings";
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
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.despawnFurthest = despawnFurthest.Value;
            despawnFurthest.SettingChanged += ABPS_SettingChanged;
            
            despawnPmcs = config.Bind(
                DespawnConfig,
                "Enable Despawning PMCs",
                false,
                new ConfigDescription("Allow ABPS to despawn PMCs. \nRequires `Enable Despawning`\n\n If you enable this and don't turn on PMC waves, then expect to have almost no PMCs in your raids. \nThat's on you.",
                    null,
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.despawnPmcs = despawnPmcs.Value;
            despawnPmcs.SettingChanged += ABPS_SettingChanged;
            
            despawnDistance = config.Bind(
                DespawnConfig,
                "Despawn Distance",
                250f,
                new ConfigDescription("Distance that bots must be from player to trigger despawning.",
                    new AcceptableValueRange<float>(100f, 500f),
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.despawnDistance = despawnDistance.Value;
            despawnDistance.SettingChanged += ABPS_SettingChanged;
            
            despawnTimer = config.Bind(
                DespawnConfig,
                "Despawn Timer",
                300f,
                new ConfigDescription("Timer for despawning, this is the MINIMUM time between despawning attempts. In Seconds.",
                    new AcceptableValueRange<float>(180f, 600f),
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.despawnTimer = despawnTimer.Value;
            despawnTimer.SettingChanged += ABPS_SettingChanged;
            
            // General Settings
            customsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Customs",
                23,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.customsMapLimit = customsMapLimit.Value;
            customsMapLimit.SettingChanged += ABPS_SettingChanged;

            factoryMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Factory",
                13,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.factoryMapLimit = factoryMapLimit.Value;
            factoryMapLimit.SettingChanged += ABPS_SettingChanged;

            interchangeMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Interchange",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.interchangeMapLimit = interchangeMapLimit.Value;
            interchangeMapLimit.SettingChanged += ABPS_SettingChanged;

            labsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Labs",
                19,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.labsMapLimit = labsMapLimit.Value;
            labsMapLimit.SettingChanged += ABPS_SettingChanged;

            lighthouseMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Lighthouse",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.lighthouseMapLimit = lighthouseMapLimit.Value;
            lighthouseMapLimit.SettingChanged += ABPS_SettingChanged;

            reserveMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Reserve",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.reserveMapLimit = reserveMapLimit.Value;
            reserveMapLimit.SettingChanged += ABPS_SettingChanged;

            groundZeroMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Ground Zero",
                16,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.groundZeroMapLimit = groundZeroMapLimit.Value;
            groundZeroMapLimit.SettingChanged += ABPS_SettingChanged;

            shorelineMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Shoreline",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.shorelineMapLimit = shorelineMapLimit.Value;
            shorelineMapLimit.SettingChanged += ABPS_SettingChanged;

            streetsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Streets",
                23,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.streetsMapLimit = streetsMapLimit.Value;
            streetsMapLimit.SettingChanged += ABPS_SettingChanged;

            woodsMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Woods",
                22,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.woodsMapLimit = woodsMapLimit.Value;
            woodsMapLimit.SettingChanged += ABPS_SettingChanged;

            labyrinthMapLimit = config.Bind(
                GeneralConfig,
                "Max Bots - Labyrinth",
                13,
                new ConfigDescription("Max bots allowed on map, value is ignored by certain bots.\nStarting PMCs ignore the cap by default, if you want to change this you must do so in the server config.\n\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 50),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.labyrinthMapLimit = labyrinthMapLimit.Value;
            labyrinthMapLimit.SettingChanged += ABPS_SettingChanged;

            progressiveChances = config.Bind(
                GeneralConfig,
                "Progressive Boss Chances",
                false,
                new ConfigDescription("Whether or not bosses will have progressive chances.\nChanges do not take effect until next raid.",
                null,
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.progressiveChances = progressiveChances.Value;
            progressiveChances.SettingChanged += ABPS_SettingChanged;

            chanceStep = config.Bind(
                GeneralConfig,
                "Progressive - Step Increase",
                5,
                new ConfigDescription("If a boss fails to spawn, how much to increase their spawn chance by.\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 15),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.chanceStep = chanceStep.Value;
            chanceStep.SettingChanged += ABPS_SettingChanged;

            minimumChance = config.Bind(
                GeneralConfig,
                "Progressive - Minimum Chance",
                5,
                new ConfigDescription("The value that a bosses chance will reset to if it spawns.\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(1, 25),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.minimumChance = minimumChance.Value;
            minimumChance.SettingChanged += ABPS_SettingChanged;

            maximumChance = config.Bind(
                GeneralConfig,
                "Progressive - Maximum Chance",
                100,
                new ConfigDescription("The maximum value that a boss can have to spawn.\nChanges do not take effect until next raid.",
                new AcceptableValueRange<int>(25, 100),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.maximumChance = maximumChance.Value;
            maximumChance.SettingChanged += ABPS_SettingChanged;

            // PMC Settings
            pmcSpawnAnywhere = config.Bind(
                PMCConfig,
                "Allow PMC Spawn Anywhere",
                false,
                new ConfigDescription("Enable this if you want PMCs to spawn at any spawn point instead of Player Spawn points.\nNote that with this disabled, PMCs will still spawn anywhere if there are no player spawn points available.",
                    null,
                    new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.pmcSpawnAnywhere = pmcSpawnAnywhere.Value;
            pmcSpawnAnywhere.SettingChanged += ABPS_SettingChanged;
            
            customs_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Customs", 
                100f, 
                new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.customs_PMCSpawnDistanceCheck = customs_PMCSpawnDistanceCheck.Value;
            customs_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            factory_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Factory", 
                30f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.factory_PMCSpawnDistanceCheck = factory_PMCSpawnDistanceCheck.Value;
            factory_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            interchange_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Interchange",
                125f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.interchange_PMCSpawnDistanceCheck = interchange_PMCSpawnDistanceCheck.Value;
            interchange_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labs_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Labs", 
                40f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.labs_PMCSpawnDistanceCheck = labs_PMCSpawnDistanceCheck.Value;
            labs_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            lighthouse_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Lighthouse",
                125f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.lighthouse_PMCSpawnDistanceCheck = lighthouse_PMCSpawnDistanceCheck.Value;
            lighthouse_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            reserve_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Reserve",
                90f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.reserve_PMCSpawnDistanceCheck = reserve_PMCSpawnDistanceCheck.Value;
            reserve_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;
            
            groundZero_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - GroundZero",
                85f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.groundZero_PMCSpawnDistanceCheck = groundZero_PMCSpawnDistanceCheck.Value;
            groundZero_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            shoreline_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Shoreline", 
                130f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.shoreline_PMCSpawnDistanceCheck = shoreline_PMCSpawnDistanceCheck.Value;
            shoreline_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            streets_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Streets", 
                120f, new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.", 
                new AcceptableValueRange<float>(10f, 175f), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.streets_PMCSpawnDistanceCheck = streets_PMCSpawnDistanceCheck.Value;
            streets_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            woods_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Woods",
                150f,
                new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.",
                new AcceptableValueRange<float>(10f, 175f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.woods_PMCSpawnDistanceCheck = woods_PMCSpawnDistanceCheck.Value;
            woods_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labyrinth_PMCSpawnDistanceCheck = config.Bind(
                PMCConfig,
                "Distance Limit - Labyrinth",
                20f,
                new ConfigDescription("How far all PMCs must be from a spawn point for it to be enabled for other PMC spawns.\n Setting this too high will cause PMCs to fail to spawn.",
                new AcceptableValueRange<float>(10f, 175f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.labyrinth_PMCSpawnDistanceCheck = labyrinth_PMCSpawnDistanceCheck.Value;
            labyrinth_PMCSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;


            // Scav Settings

            softCap = config.Bind(
                ScavConfig, 
                "Scav Soft Cap", 
                3, 
                new ConfigDescription("How many open slots before hard cap to stop spawning additional scavs.\nEx..If 3, and map cap is 23 - will stop spawning scavs at 20 total.\nThis allows PMC waves if enabled to fill the remaining spots.", 
                new AcceptableValueRange<int>(0, 10), 
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.softCap = softCap.Value;
            softCap.SettingChanged += ABPS_SettingChanged;

            pScavChance = config.Bind(
                ScavConfig, 
                "PScav Chance", 
                20, 
                new ConfigDescription("How likely a scav spawning later in the raid is a Player Scav.",
                new AcceptableValueRange<int>(0, 100),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.pScavChance = pScavChance.Value;
            pScavChance.SettingChanged += ABPS_SettingChanged;

            zoneScavCap = config.Bind(
                ScavConfig,
                "Zone Cap",
                2,
                new ConfigDescription("How many scavs can spawn in any one zone (excluding Factory/Ground Zero).",
                new AcceptableValueRange<int>(0, 15),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.zoneScavCap = zoneScavCap.Value;
            zoneScavCap.SettingChanged += ABPS_SettingChanged;

            enableHotzones = config.Bind(
                ScavConfig,
                "Hotzones",
                false,
                new ConfigDescription("Enables hotzones around maps, more common or quest areas are considered hotzones.",
                null,
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.enableHotzones = enableHotzones.Value;
            enableHotzones.SettingChanged += ABPS_SettingChanged;

            hotzoneScavCap = config.Bind(
                ScavConfig,
                "Hotzone Cap",
                4,
                new ConfigDescription("How many scavs can spawn in a hotzone, if you enable hotzones (excluding Factory/Ground Zero).",
                new AcceptableValueRange<int>(0, 15),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.hotzoneScavCap = hotzoneScavCap.Value;
            hotzoneScavCap.SettingChanged += ABPS_SettingChanged;

            hotzoneScavChance = config.Bind(
                ScavConfig,
                "Hotzone Chance",
                20,
                new ConfigDescription("How likely a scav is to spawn in a hotzone (excluding Factory/Ground Zero).",
                new AcceptableValueRange<int>(0, 100),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.hotzoneScavChance = hotzoneScavChance.Value;
            hotzoneScavChance.SettingChanged += ABPS_SettingChanged;

            customs_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig, 
                "Distance Limit - Customs", 
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(5f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.customs_ScavSpawnDistanceCheck = customs_ScavSpawnDistanceCheck.Value;
            customs_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            factory_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig, 
                "Distance Limit - Factory", 
                30f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(5f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.factory_ScavSpawnDistanceCheck = factory_ScavSpawnDistanceCheck.Value;
            factory_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            interchange_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Interchange",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.interchange_ScavSpawnDistanceCheck = interchange_ScavSpawnDistanceCheck.Value;
            interchange_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labs_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Labs", 
                40f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.labs_ScavSpawnDistanceCheck = labs_ScavSpawnDistanceCheck.Value;
            labs_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            lighthouse_ScavSpawnDistanceCheck = config.Bind
                (ScavConfig,
                "Distance Limit - Lighthouse",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.lighthouse_ScavSpawnDistanceCheck = lighthouse_ScavSpawnDistanceCheck.Value;
            lighthouse_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            reserve_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Reserve",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.reserve_ScavSpawnDistanceCheck = reserve_ScavSpawnDistanceCheck.Value;
            reserve_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            groundZero_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - GroundZero",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.groundZero_ScavSpawnDistanceCheck = groundZero_ScavSpawnDistanceCheck.Value;
            groundZero_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            shoreline_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Shoreline",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.shoreline_ScavSpawnDistanceCheck = shoreline_ScavSpawnDistanceCheck.Value;
            shoreline_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            streets_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Streets",
                45f, 
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.streets_ScavSpawnDistanceCheck = streets_ScavSpawnDistanceCheck.Value;
            streets_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            woods_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Woods",
                45f,
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.woods_ScavSpawnDistanceCheck = woods_ScavSpawnDistanceCheck.Value;
            woods_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;

            labyrinth_ScavSpawnDistanceCheck = config.Bind(
                ScavConfig,
                "Distance Limit - Woods",
                45f,
                new ConfigDescription("How far PMCs must be from a spawn point for it to be enabled for Scav spawns.\n Setting this too high will cause Scavs to fail to spawn.",
                new AcceptableValueRange<float>(1f, 50f),
                new ConfigurationManagerAttributes { Order = loadOrder-- }));
            Plugin.labyrinth_ScavSpawnDistanceCheck = labyrinth_ScavSpawnDistanceCheck.Value;
            labyrinth_ScavSpawnDistanceCheck.SettingChanged += ABPS_SettingChanged;
        }
        private static void ABPS_SettingChanged(object sender, EventArgs e)
        {
            Plugin.despawnFurthest = despawnFurthest.Value;
            Plugin.despawnDistance = despawnDistance.Value;
            
            Plugin.customsMapLimit = customsMapLimit.Value;
            Plugin.factoryMapLimit = factoryMapLimit.Value;
            Plugin.interchangeMapLimit = interchangeMapLimit.Value;
            Plugin.labsMapLimit = labsMapLimit.Value;
            Plugin.lighthouseMapLimit = lighthouseMapLimit.Value;
            Plugin.reserveMapLimit = reserveMapLimit.Value;
            Plugin.groundZeroMapLimit = groundZeroMapLimit.Value;
            Plugin.shorelineMapLimit = shorelineMapLimit.Value;
            Plugin.streetsMapLimit = streetsMapLimit.Value;
            Plugin.woodsMapLimit = woodsMapLimit.Value;
            Plugin.labyrinthMapLimit = labyrinthMapLimit.Value;

            Plugin.progressiveChances = progressiveChances.Value;
            Plugin.chanceStep = chanceStep.Value;
            Plugin.minimumChance = minimumChance.Value;
            Plugin.maximumChance = maximumChance.Value;

            Plugin.pmcSpawnAnywhere = pmcSpawnAnywhere.Value;
            Plugin.customs_PMCSpawnDistanceCheck = customs_PMCSpawnDistanceCheck.Value;
            Plugin.factory_PMCSpawnDistanceCheck = factory_PMCSpawnDistanceCheck.Value;
            Plugin.interchange_PMCSpawnDistanceCheck = interchange_PMCSpawnDistanceCheck.Value;
            Plugin.labs_PMCSpawnDistanceCheck = labs_PMCSpawnDistanceCheck.Value;
            Plugin.lighthouse_PMCSpawnDistanceCheck = lighthouse_PMCSpawnDistanceCheck.Value;
            Plugin.reserve_PMCSpawnDistanceCheck = reserve_PMCSpawnDistanceCheck.Value;
            Plugin.groundZero_PMCSpawnDistanceCheck = groundZero_PMCSpawnDistanceCheck.Value;
            Plugin.shoreline_PMCSpawnDistanceCheck = shoreline_PMCSpawnDistanceCheck.Value;
            Plugin.streets_PMCSpawnDistanceCheck = streets_PMCSpawnDistanceCheck.Value;
            Plugin.woods_PMCSpawnDistanceCheck = woods_PMCSpawnDistanceCheck.Value;
            Plugin.labyrinth_PMCSpawnDistanceCheck = labyrinth_PMCSpawnDistanceCheck.Value;

            Plugin.softCap = softCap.Value;
            Plugin.pScavChance = pScavChance.Value;
            Plugin.enableHotzones = enableHotzones.Value;
            Plugin.zoneScavCap = zoneScavCap.Value;
            Plugin.hotzoneScavCap = hotzoneScavCap.Value;
            Plugin.customs_ScavSpawnDistanceCheck = customs_ScavSpawnDistanceCheck.Value;
            Plugin.factory_ScavSpawnDistanceCheck = factory_ScavSpawnDistanceCheck.Value;
            Plugin.interchange_ScavSpawnDistanceCheck = interchange_ScavSpawnDistanceCheck.Value;
            Plugin.labs_ScavSpawnDistanceCheck = labs_ScavSpawnDistanceCheck.Value;
            Plugin.lighthouse_ScavSpawnDistanceCheck = lighthouse_ScavSpawnDistanceCheck.Value;
            Plugin.reserve_ScavSpawnDistanceCheck = reserve_ScavSpawnDistanceCheck.Value;
            Plugin.groundZero_ScavSpawnDistanceCheck = groundZero_ScavSpawnDistanceCheck.Value;
            Plugin.shoreline_ScavSpawnDistanceCheck = shoreline_ScavSpawnDistanceCheck.Value;
            Plugin.streets_ScavSpawnDistanceCheck = streets_ScavSpawnDistanceCheck.Value;
            Plugin.woods_ScavSpawnDistanceCheck = woods_ScavSpawnDistanceCheck.Value;
            Plugin.labyrinth_ScavSpawnDistanceCheck = labyrinth_ScavSpawnDistanceCheck.Value;
        }
    }
}
