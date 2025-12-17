using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Utils;

public class BotZoneVisualizer : MonoBehaviour
{
    
    private List<BotZone> _botZones = [];
    
    public void Awake()
    {
        if (Plugin.botSpawnerInstance is not null)
        {
            Plugin.LogSource.LogInfo("BotSpawner was not null - trying to visualize botzones");
            _botZones = Plugin.botSpawnerInstance.AllBotZones.ToList();

            foreach (var botZone in _botZones)
            {
                var gameObject = botZone.gameObject;
                gameObject.AddComponent<BoundsVisualizer>();
            }
                
        }
        else
        {
            Plugin.LogSource.LogInfo("BotSpawner was null");
        }
    }
}