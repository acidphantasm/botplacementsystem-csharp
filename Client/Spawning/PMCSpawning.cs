using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Spawning
{
    public class PmcGroupSpawner : MonoBehaviour
    {
        private static BossSpawnerClass _bossSpawnerClass;
        private static BotSpawner _botSpawner;
        private static IBotCreator _iBotCreator;
        
        public static readonly Dictionary<string, HashSet<string>> AllPmcGroups = new Dictionary<string, HashSet<string>>();
        private static readonly Dictionary<string, string> FollowerToLeader = new Dictionary<string, string>();
        private static readonly Dictionary<string, BossSpawnerClass.Class332> WavePmcGroupClassData = new Dictionary<string, BossSpawnerClass.Class332>();
        
        public static async Task StartSpawnPMCGroup(BotCreationDataClass creationData, BossLocationSpawn wave, BotSpawnParams spawnParams, int followersCount, BotZone botZone, List<ISpawnPoint> openedPositions, BossSpawnerClass bossSpawnerClass, BotSpawner botSpawner, IBotCreator botCreator)
        {
            _bossSpawnerClass ??= bossSpawnerClass;
            _botSpawner ??= botSpawner;
            _iBotCreator ??= botCreator;

            BossSpawnerClass.Class332 @class = new BossSpawnerClass.Class332();
            @class.BossSpawnerClass = _bossSpawnerClass;
            @class.creationData = creationData;
            @class.botZone = botZone;
            @class.followersCount = followersCount;
            @class.spawnParams = spawnParams;
            @class.wave = wave;
            @class.openedPositions = openedPositions;
            float time = @class.wave.Time;
            @class.spawnParams.ShallBeGroup = new ShallBeGroupParams(true, true, @class.followersCount + 1);
            BotProfileDataClass botProfileDataClass = new BotProfileDataClass(EPlayerSide.Savage, @class.wave.BossType, @class.wave.BossDif, time, @class.spawnParams);
            @class.side = EPlayerSide.Savage;
            bool flag = @class.wave.IsStartWave();
            ISpawnPoint spawnPoint = @class.openedPositions[0];
            @class.openedPositions.Remove(spawnPoint);
            @class.spawnProcessData = new BossSpawnerClass.GClass669(@class.wave, @class.botZone, spawnPoint);
            _bossSpawnerClass.List_0.Add(@class.spawnProcessData);

            var leaderProfileId = creationData.Profiles[0].ProfileId;

            // Initialize the leader's entry in the group map (no followers yet)
            if (!AllPmcGroups.ContainsKey(leaderProfileId))
                AllPmcGroups[leaderProfileId] = new HashSet<string>();

            if (flag)
            {
                if (_bossSpawnerClass.BotSpawner_0.CanSpawnRole(botProfileDataClass))
                {
                    SpawnLeader(@class.creationData, spawnPoint, @class.botZone, @class.followersCount, botProfileDataClass, new Action<BotOwner>(@class.method_0));
                    await SpawnFollowers(@class.creationData, @class.botZone, @class.followersCount, @class.spawnParams, @class.wave, @class.side, @class.openedPositions, true, leaderProfileId);
                }
            }
            else
            {
                // OG WAVE - No follower & spawn leader is method_1 callback instead
                if (followersCount != 0) WavePmcGroupClassData[leaderProfileId] = @class;
                SpawnLeader(@class.creationData, spawnPoint, @class.botZone, @class.followersCount, botProfileDataClass, new Action<BotOwner>(@class.method_0));
            }
        }

        private static void SpawnLeader(BotCreationDataClass creationData, ISpawnPoint point, BotZone ss, int followers, BotProfileDataClass data, Action<BotOwner> callback)
        {
            BossSpawnerClass.Class335 @class = new BossSpawnerClass.Class335();
            @class.data = data;
            @class.followers = followers;
            @class.callback = callback;
            List<ISpawnPoint> list = new List<ISpawnPoint> { point };
            SpawnBotsInZoneOnPositions(list, ss, creationData, new Action<BotOwner>(@class.method_0));
        }

        private static async Task SpawnFollowers(BotCreationDataClass bossCreationData, BotZone zone, int followersCount, BotSpawnParams spawnParams, BossLocationSpawn wave, EPlayerSide side, List<ISpawnPoint> pointsToSpawn, bool forceSpawn, string leaderProfileId)
        {
            List<BossLocationSpawnSubData> escors = wave.GetEscors();
            if (escors != null)
            {
                GenerateFollowerData(bossCreationData, zone, side, wave, escors, spawnParams, pointsToSpawn, forceSpawn, leaderProfileId).HandleExceptions();
            }
            else if (followersCount > 0)
            {
                BotCreationDataClass botCreationDataClass = await BotCreationDataClass.Create(new BotProfileDataClass(EPlayerSide.Savage, wave.EscortType, wave.EscortDif, wave.Time, spawnParams, false), _iBotCreator, followersCount, _botSpawner);
                RegisterFollowers(leaderProfileId, botCreationDataClass.Profiles);
                TryToSpawnInZoneAndDelay(zone, botCreationDataClass, false, true, pointsToSpawn, forceSpawn);
            }
        }

        private static async Task GenerateFollowerData(BotCreationDataClass creationData, BotZone zone, EPlayerSide side, BossLocationSpawn wave, List<BossLocationSpawnSubData> escorts, BotSpawnParams spawnParams, List<ISpawnPoint> pointsToSpawn, bool forceSpawn, string leaderProfileId)
        {
            if (wave.EscortCount > pointsToSpawn.Count)
            {
                pointsToSpawn = null;
            }
            foreach (BossLocationSpawnSubData bossLocationSpawnSubData in escorts)
            {
                List<ISpawnPoint> list = null;
                if (pointsToSpawn != null)
                {
                    list = new List<ISpawnPoint>();
                    for (int i = 0; i < bossLocationSpawnSubData.BossEscortAmount; i++)
                    {
                        if (pointsToSpawn.Count > 0)
                        {
                            ISpawnPoint spawnPoint = pointsToSpawn.First<ISpawnPoint>();
                            list.Add(spawnPoint);
                            pointsToSpawn.Remove(spawnPoint);
                        }
                    }
                    if (bossLocationSpawnSubData.BossEscortAmount != list.Count)
                    {
                        list = null;
                    }
                }
                BotCreationDataClass result = await BotCreationDataClass.Create(new BotProfileDataClass(side, bossLocationSpawnSubData.BossEscortType, bossLocationSpawnSubData.EscortDifficulty, wave.Time, spawnParams), _iBotCreator, bossLocationSpawnSubData.BossEscortAmount, _botSpawner);

                RegisterFollowers(leaderProfileId, result.Profiles);

                TryToSpawnInZoneAndDelay(zone, result, false, true, list, forceSpawn);
                await Task.Yield();
                list = null;
            }
        }

        private static void RegisterFollowers(string leaderProfileId, IEnumerable<Profile> profiles)
        {
            if (!AllPmcGroups.TryGetValue(leaderProfileId, out var followerSet))
            {
                followerSet = new HashSet<string>();
                AllPmcGroups[leaderProfileId] = followerSet;
            }

            foreach (var profile in profiles)
            {
                followerSet.Add(profile.ProfileId);
                FollowerToLeader[profile.ProfileId] = leaderProfileId;
            }
        }

        private static void SpawnBotsInZoneOnPositions(List<ISpawnPoint> openedPositions, BotZone botZone, BotCreationDataClass data, Action<BotOwner> callback = null)
        {
            AddSpawnPointDataAndSpawn(openedPositions, botZone, data, callback, _botSpawner.CancellationTokenSource.Token).HandleExceptions();
        }

        private static async Task AddSpawnPointDataAndSpawn(List<ISpawnPoint> spawnPoints, BotZone botZone, BotCreationDataClass data, Action<BotOwner> callback, CancellationToken cancellationToken)
        {
            _botSpawner.InSpawnProcess += spawnPoints.Count;
            if (!data.SpawnStopped)
            {
                int maxBots = _botSpawner.MaxBots;
                if (!cancellationToken.IsCancellationRequested)
                {
                    foreach (ISpawnPoint spawnPoint in spawnPoints)
                    {
                        if (spawnPoint.Categories.ContainPlayerCategory())
                        {
                            var corePointID = Singleton<IBotGame>.Instance.BotsController.CoversData.GetClosest(spawnPoint.Position).CorePointInGame.Id;
                            data.AddPosition(spawnPoint.Position, corePointID);
                        }
                        else
                        {
                            data.AddPosition(spawnPoint.Position, spawnPoint.CorePointId);
                        }
                    }
                    spawnPoints.Clear();
                    SpawnBot(botZone, data, callback, cancellationToken);
                    await Task.Yield();
                }
            }
        }

        private static void TryToSpawnInZoneAndDelay(BotZone botZone, BotCreationDataClass data, bool withCheckMinMax, bool newWave, List<ISpawnPoint> pointsToSpawn = null, bool forcedSpawn = false)
        {
            if (data.SpawnStopped)
            {
                return;
            }
            GClass1884 gclass = TryToSpawnInZoneInner(botZone, data, data.Count, withCheckMinMax, newWave, pointsToSpawn, forcedSpawn);
            /* REMOVE DELAY SERVICES CAUSE THIS MAKES BOTS WAIT FOOOOOOOREVER TO SPAWN
            if (gclass != null)
            {
                _botSpawner.SpawnDelaysService.Add(gclass);
            }
            */
        }

        private static GClass1884 TryToSpawnInZoneInner(BotZone botZone, BotCreationDataClass data, int count, bool withCheckMinMax, bool newWave, List<ISpawnPoint> pointsToSpawn = null, bool forcedSpawn = false)
        {
            if (data.SpawnStopped)
            {
                return null;
            }
            if (DebugBotData.UseDebugData && DebugBotData.Instance.spawnInstantly)
            {
                forcedSpawn = true;
            }
            if (!_botSpawner.BotCreator.StartProfilesLoaded)
            {
                return new GClass1884(botZone, count, data, new Action<GClass1884>(_botSpawner.method_8));
            }
            if (DebugBotData.UseDebugData && DebugBotData.Instance.spawnInstantly)
            {
                List<ISpawnPoint> array = _botSpawner.SpawnSystem.SelectAISpawnPoints(data, botZone, count, null, ActionIfNotEnoughPoints.DuplicateIfAtLeastOne);
                SpawnBotsInZoneOnPositions(array, botZone, data, null);
                return new GClass1884(botZone, 0, data, new Action<GClass1884>(_botSpawner.method_8));
            }
            if (!data.CanAtZoneByType(botZone, _botSpawner.BotGame.BotsController.ZonesLeaveController))
            {
                return new GClass1884(botZone, count, data, new Action<GClass1884>(_botSpawner.method_8));
            }
            _botSpawner.Bots.GetListByZone(botZone);
            bool flag = data.IsBossOrFollowerByTime();
            if (withCheckMinMax && !botZone.HaveFreeSpace(count) && !flag && !forcedSpawn)
            {
                return new GClass1884(botZone, count, data, new Action<GClass1884>(_botSpawner.method_8));
            }
            if (newWave)
            {
                Action<GClass1888> onSpawnedWave = (x) => new GClass1888(botZone, count, data);
                _botSpawner.OnSpawnedWave += onSpawnedWave;
            }
            int num;
            int num2;
            if (withCheckMinMax && !forcedSpawn)
            {
                _botSpawner.CheckOnMax(count, out num, out num2, false);
            }
            else
            {
                num = 0;
                num2 = count;
            }
            if (num > 0)
            {
                return new GClass1884(botZone, num, data, new Action<GClass1884>(_botSpawner.method_8));
            }
            if (num2 > 0)
            {
                if (flag)
                {
                    data.IsSpawnOnStart();
                }
                count = num2;
                List<ISpawnPoint> list2;
                if (pointsToSpawn != null)
                {
                    list2 = pointsToSpawn;
                }
                else
                {
                    list2 = _botSpawner.SpawnSystem.SelectAISpawnPoints(data, botZone, count, null, ActionIfNotEnoughPoints.DuplicateIfAtLeastOne);
                    if (count > list2.Count)
                    {
                        if (!forcedSpawn)
                        {
                            int num3 = count - list2.Count;
                            return new GClass1884(botZone, num3, data, new Action<GClass1884>(_botSpawner.method_8));
                        }
                        list2 = _botSpawner.SpawnSystem.SelectAISpawnPoints(data, botZone, count, null, ActionIfNotEnoughPoints.ReturnFoundPoints);
                    }
                }
                SpawnBotsInZoneOnPositions(list2, botZone, data, null);
            }
            return null;
        }

        private static void SpawnBot(BotZone zone, BotCreationDataClass data, Action<BotOwner> callback, CancellationToken cancellationToken)
        {
            BotSpawner.Class1164 @class = new BotSpawner.Class1164();
            @class.botSpawner_0 = _botSpawner;
            @class.data = data;
            @class.callback = callback;
            if (_botSpawner.GameEnd)
            {
                return;
            }
            if (@class.data.SpawnStopped)
            {
                _botSpawner.InSpawnProcess--;
                return;
            }
            @class.stopWatch = new Stopwatch();
            @class.stopWatch.Start();
            @class.shallBeGroup = @class.data.SpawnParams != null && @class.data.SpawnParams.ShallBeGroup != null && @class.data.SpawnParams.ShallBeGroup.Group && @class.data.SpawnParams.ShallBeGroup.RemainCount > 0;
            if (@class.shallBeGroup)
            {
                @class.data.SpawnParams.ShallBeGroup.DescreaseCount();
            }
            _botSpawner.BotCreator.ActivateBot(@class.data, zone, @class.shallBeGroup, new Func<BotOwner, BotZone, BotsGroup>(GetGroupAndSetEnemies), new Action<BotOwner>(@class.method_0), cancellationToken);

            var spawnedBotProfileId = data.Profiles[0].ProfileId;
            if (!WavePmcGroupClassData.TryGetValue(spawnedBotProfileId, out var originalClassData)) return;

            // Spawn boss followers now
            SpawnFollowers(@originalClassData.creationData, @originalClassData.botZone, @originalClassData.followersCount, @originalClassData.spawnParams, @originalClassData.wave, @originalClassData.side, @originalClassData.openedPositions, true, spawnedBotProfileId);
        }

        private static BotsGroup GetGroupAndSetEnemies(BotOwner bot, BotZone zone)
        {
            var side = bot.Profile.Info.Side;
            var botProfileId = bot.Profile.ProfileId;
            List<BotOwner> botOwners = new List<BotOwner>();

            if (AllPmcGroups.ContainsKey(botProfileId))
            {
                foreach (BotOwner botOwner in _botSpawner.method_5(bot))
                {
                    botOwners.Add(botOwner);
                }
                _botSpawner.method_4(bot);
                BotsGroup botsGroup = new BotsGroup(zone, _botSpawner.BotGame, bot, botOwners, _botSpawner.DeadBodiesController, _botSpawner.AllPlayers, true);
                if (bot.SpawnProfileData.SpawnParams.ShallBeGroup != null)
                {
                    botsGroup.TargetMembersCount = bot.SpawnProfileData.SpawnParams.ShallBeGroup.StartCount;
                }
                _botSpawner.Groups.Add(zone, side, botsGroup, true);
                return botsGroup;
            }

            if (FollowerToLeader.TryGetValue(botProfileId, out var leaderId))
            {
                foreach (var (botZone, botGroup) in _botSpawner.Groups)
                {
                    foreach (var group in botGroup.HashSet_0)
                    {
                        if (group.InitialBot.ProfileId == leaderId)
                        {
                            _botSpawner.method_4(bot);
                            return group;
                        }
                    }
                }
            }

            return null;
        }
    }
}