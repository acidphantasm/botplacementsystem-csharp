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
        public static Dictionary<string, CustomizedObject> BossInfoOutOfRaid { get; set; } = [];
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
            CustomizedObject values = new CustomizedObject();
            string bossName = boss.ToString();

            values.SpawnedLastRaid = true;
            values.Chance = Plugin.minimumChance;

            if (!BossInfoForProfile.ContainsKey(bossName)) BossInfoForProfile.Add(bossName, values);
            else
            {
                BossInfoForProfile[bossName].SpawnedLastRaid = values.SpawnedLastRaid;
                BossInfoForProfile[bossName].Chance = values.Chance;
            }

        }
        public static void EndRaidMergeData()
        {
            BossInfoOutOfRaid = BossInfoForProfile;
            SaveRaidEndInServer();
        }

        public static void SaveRaidEndInServer()
        {
            try
            {
                var profile = Utility.GetPlayerProfile().ProfileId;
                
                var jsonString = JsonConvert.SerializeObject(
                    new RequestData() { Data = BossInfoOutOfRaid, ProfileId = profile }, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                
                RequestHandler.PutJsonAsync("/botplacementsystem/save", jsonString);
                BossInfoForProfile.Clear();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError("Failed to save: " + ex.ToString());
                NotificationManagerClass.DisplayWarningNotification("Failed to save ABPS Progressive Boss data - check the server");
            }
        }

        public static async Task LoadFromServer()
        {
            try
            {
                var profile = Utility.GetPlayerProfile().ProfileId;
                
                string payload = await RequestHandler.GetJsonAsync("/botplacementsystem/load");
                var retrievedData =
                    JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, CustomizedObject>>>(payload);

                BossInfoForProfile = retrievedData.TryGetValue(profile, out var value) ? value : new Dictionary<string, CustomizedObject>();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError("Failed to load: " + ex.ToString());
                NotificationManagerClass.DisplayWarningNotification("Failed to load ABPS Progressive Boss data - check the server");
            }
        }

        public class CustomizedObject
        {
            public bool SpawnedLastRaid;
            public int Chance;
        }
        
        private struct RequestData
        {
            public string ProfileId;
            public Dictionary<string, CustomizedObject> Data;
        }
    }
}
