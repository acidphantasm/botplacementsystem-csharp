using acidphantasm_botplacementsystem.Utils;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace acidphantasm_botplacementsystem.Spawning
{
    public class BossSpawnTracking
    {
        public static Dictionary<string, Dictionary<string, CustomizedObject>> BossInfoOutOfRaid { get; set; } = [];
        public static Dictionary<string, CustomizedObject> BossInfoForProfile { get; set; } = [];

        public static HashSet<WildSpawnType> TrackedBosses = new HashSet<WildSpawnType>
        {
            WildSpawnType.bossBoar,
            WildSpawnType.bossBully,
            WildSpawnType.bossGluhar,
            WildSpawnType.bossKilla,
            WildSpawnType.bossKnight,
            WildSpawnType.bossKolontay,
            WildSpawnType.bossKojaniy,
            WildSpawnType.bossSanitar,
            WildSpawnType.bossTagilla,
            WildSpawnType.bossPartisan,
            WildSpawnType.bossZryachiy,
            WildSpawnType.arenaFighterEvent,
            WildSpawnType.sectantPriest,
        };

        /*
         * 
         *  (WildSpawnType) 199,                // Legion
         *  (WildSpawnType) 801,                // Punisher
        */

        public static void UpdateBossSpawnChance(WildSpawnType boss)
        {
            var profileID = Utility.GetPlayerProfile().ProfileId;
            CustomizedObject values = new CustomizedObject();
            string bossName = boss.ToString();

            values.spawnedLastRaid = true;
            values.chance = Plugin.minimumChance;

            if (!BossInfoForProfile.ContainsKey(bossName)) BossInfoForProfile.Add(bossName, values);
            else
            {
                BossInfoForProfile[bossName].spawnedLastRaid = values.spawnedLastRaid;
                BossInfoForProfile[bossName].chance = values.chance;
            }

        }
        public static void EndRaidMergeData()
        {
            var profileID = Utility.GetPlayerProfile().ProfileId;
            BossInfoOutOfRaid[profileID] = BossInfoForProfile;
            SaveRaidEndInServer();
        }

        public static void SaveRaidEndInServer()
        {
            try
            {
                RequestHandler.PutJsonAsync("/abps/save", JsonConvert.SerializeObject(BossInfoOutOfRaid));

            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError("Failed to save: " + ex.ToString());
                NotificationManagerClass.DisplayWarningNotification("Failed to save Boss Tracking data - check the server");
            }
        }

        public static async Task LoadFromServer()
        {
            try
            {
                string payload = await RequestHandler.GetJsonAsync("/abps/load");
                BossInfoOutOfRaid = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, CustomizedObject>>>(payload);

                var profileID = Utility.GetPlayerProfile().ProfileId;
                if (BossInfoOutOfRaid.ContainsKey(profileID))
                {
                    BossInfoForProfile = BossInfoOutOfRaid[profileID];
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError("Failed to load: " + ex.ToString());
                NotificationManagerClass.DisplayWarningNotification("Failed to load Boss Tracking data - check the server");
            }
        }

        public class CustomizedObject
        {
            public bool spawnedLastRaid;
            public int chance;
        }
    }
}
