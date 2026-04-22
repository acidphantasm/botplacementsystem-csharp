using acidphantasm_botplacementsystem.Utils;
using EFT;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using Systems.Effects;
using UnityEngine;
using Object = System.Object;

namespace acidphantasm_botplacementsystem.Patches
{
    internal class NonWavesSpawnSystemPatch : ModulePatch
    {
        private static float _nextDespawnCheckTime = 0f;
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NonWavesSpawnScenario), nameof(NonWavesSpawnScenario.Update));
        }

        [PatchPrefix]
        private static bool PatchPrefix(
            NonWavesSpawnScenario __instance,
            ref BotsController ___botsController_0,
            ref AbstractGame ___abstractGame_0,
            ref LocationSettingsClass.Location ___location_0,
            ref GClass1881<BotDifficulty> ___gclass1881_0,
            ref GClass1881<WildSpawnType> ___gclass1881_1,
            ref GClass1876 ___gclass1876_0,
            ref bool ___bool_0,
            ref bool ___bool_1,
            ref bool ___bool_2,
            ref float ___nullable_0,
            ref float ___float_2,
            ref float ___float_0)
        {
            ref var isActive = ref ___bool_1;
            ref var isAtBotCap = ref ___bool_0;
            ref var isInSpawnWindow = ref ___bool_2;
            ref var nextWindowToggleTime = ref ___nullable_0;
            ref var spawnAttemptInterval = ref ___float_2;
            ref var lastSpawnAttemptTime = ref ___float_0;
            ref GClass1881<BotDifficulty> difficultyWeights = ref ___gclass1881_0;

            if (__instance == null || !isActive) return true;

            if (___abstractGame_0.PastTime < (float)___location_0.BotStart || ___abstractGame_0.PastTime > (float)___location_0.BotStop)
                return false;

            if (nextWindowToggleTime.Equals(null) || nextWindowToggleTime <= ___abstractGame_0.PastTime)
            {
                isInSpawnWindow = !isInSpawnWindow;
                nextWindowToggleTime = ___abstractGame_0.PastTime + (isInSpawnWindow
                    ? GClass856.Random((float)___location_0.BotSpawnTimeOnMin, (float)___location_0.BotSpawnTimeOnMax)
                    : GClass856.Random((float)___location_0.BotSpawnTimeOffMin, (float)___location_0.BotSpawnTimeOffMax));
            }

            if (!isInSpawnWindow)
            {
                return false;
            }

            if (___abstractGame_0.PastTime - lastSpawnAttemptTime < spawnAttemptInterval)
            {
                return false;
            }
            lastSpawnAttemptTime = ___abstractGame_0.PastTime;

            var freeSlots = (___botsController_0.MaxCount - Plugin.SoftCap) - ___botsController_0.AliveLoadingDelayedBotsCount;

            if (isAtBotCap)
            {
                if (freeSlots < ___location_0.BotSpawnCountStep)
                {
                    return false;
                }
                spawnAttemptInterval = 15f;
                isAtBotCap = false;
            }
            else if (freeSlots <= 0)
            {
                spawnAttemptInterval = Math.Max((float)___location_0.BotSpawnPeriodCheck, 15f);
                isAtBotCap = ___botsController_0.MaxCount - ___botsController_0.AliveAndLoadingBotsCount <= 0;
                return false;
            }
            
            if (Utility.BotsSpawnedPerPlayer >= ___botsController_0.BotLocationModifier.NonWaveSpawnBotsLimitPerPlayerPvE)
            {
                return false;
            }
            
            var mapName = Utility.CurrentLocation.ToLower();
            if (Utility.CurrentMapZones.Count == 0)
            {
                Utility.CurrentMapZones = ___botsController_0.BotSpawner.AllBotZones.ToList();
            }

            var botZone = GetValidBotZone(WildSpawnType.assault, 1, ___botsController_0.BotSpawner.AllBotZones, mapName, ___botsController_0);
            ___botsController_0.ActivateBotsByWave(new BotWaveDataClass
            {
                BotsCount = 1,
                Time = Time.time,
                Difficulty = ___gclass1881_0.Random(),
                IsPlayers = GClass856.IsTrue100(Plugin.PScavChance),
                Side = EPlayerSide.Savage,
                WildSpawnType = WildSpawnType.assault,
                SpawnAreaName = botZone,
                WithCheckMinMax = false,
                ChanceGroup = 0,
            });
            Utility.BotsSpawnedPerPlayer += 1d / Math.Max(1, Utility.CachedConnectedPlayers.Count);

            if (!(Time.time >= _nextDespawnCheckTime) || !Plugin.DespawnFurthest)
            {
                return false;
            }
            
            _nextDespawnCheckTime = Time.time + Plugin.DespawnTimer;
            var center = GetPlayerCountAndCenter();
            DespawnFurthestBots(___botsController_0, center);

            return false;
        }
        
        private static void DespawnFurthestBots(BotsController botsController, Vector3 centerOfPlayers)
        {
            var despawnDistance = Plugin.DespawnDistance;

            if (Plugin.DespawnPmcs)
            {
                foreach (var pmc in Utility.CachedPmcs)
                {
                    if (pmc == null || !pmc.HealthController.IsAlive) continue;
                    if (Vector3.Distance(pmc.Position, centerOfPlayers) >= despawnDistance)
                        AttemptToDespawnBot(botsController, pmc.AIData.BotOwner);
                }
            }

            foreach (var scav in Utility.CachedAssaultBots)
            {
                if (scav == null || !scav.HealthController.IsAlive) continue;
                if (Vector3.Distance(scav.Position, centerOfPlayers) >= despawnDistance)
                    AttemptToDespawnBot(botsController, scav.AIData.BotOwner);
            }
        }
        
        private static Vector3 GetPlayerCountAndCenter()
        {
            var centerPoint = Vector3.zero;
            var count = 0;

            foreach (var player in Utility.CachedConnectedPlayers)
            {
                if (player == null || !player.HealthController.IsAlive)
                {
                    continue;
                }
                
                centerPoint += player.Position;
                count++;
            }

            return count == 0 ? centerPoint : centerPoint / count;
        }

        private static void AttemptToDespawnBot(BotsController botsController, BotOwner botToDespawn)
        {
            var effectsCommutator = Singleton<Effects>.Instance.EffectsCommutator;
            var gameWorld = Singleton<GameWorld>.Instance;

            if (effectsCommutator is null || gameWorld is null) return;

            var botPlayer = botToDespawn.GetPlayer;
            
            effectsCommutator.StopBleedingForPlayer(botPlayer);
            gameWorld.UnregisterPlayer(botToDespawn);
            gameWorld.UnregisterPlayer(botPlayer);
            botToDespawn.Deactivate();
            botToDespawn.Dispose();
            botsController.BotDied(botToDespawn);
            botsController.DestroyInfo(botPlayer);
            UnityEngine.Object.DestroyImmediate(botToDespawn.gameObject);
            UnityEngine.Object.Destroy(botToDespawn);
        }
        
        private static string GetValidBotZone(WildSpawnType botType, int count, BotZone[] allZones, string location, BotsController _botsController)
        {
            if (Utility.CachedNonSnipeZones == null || Utility.CachedNonSnipeZones.Count == 0)
            {
                Utility.CachedNonSnipeZones = allZones.Where(x => !x.SnipeZone).ToList();
            }
            var botZones = Utility.CachedNonSnipeZones.OrderBy(_ => GClass856.Random(0f, 1f)).ToList();

            if (Plugin.EnableHotzones && GClass856.IsTrue100(Plugin.HotzoneScavChance) && Utility.MapHotSpots.ContainsKey(location))
            {
                var hotSpotZone = Utility.MapHotSpots[location].RandomElement();
                return hotSpotZone;
            }
            foreach (var currentZone in botZones)
            {
                if (_botsController.Bots.GetListByZone(currentZone).Count(x => x.IsRole(WildSpawnType.assault)) < Plugin.ZoneScavCap)
                {
                    return currentZone.NameZone;
                }
            }

            return "";
        }
    }
}
