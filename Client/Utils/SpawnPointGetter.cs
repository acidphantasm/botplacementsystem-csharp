using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace acidphantasm_botplacementsystem.Utils
{
    public class SpawnPointGetter : MonoBehaviour
    {
        private List<SpawnPointMarker> _spawnPointMarkers = [];
        private List<SpawnPointInfo> _spawnPointInfo = [];

        private StringBuilder _sb = new();
        private GUIStyle guiStyle;
        private float _screenScale = 1.0f;


        /*
         * Code in here is based on dirtbikercj's DebugPlus with some minor fixes, and primarily fixed to show all spawn points, and really all of them.
         * This should never be instatiated, or called, outside of a development environment in very specific scenarios
         * If you're mucking around in here, good luck
         */
        private static Player _player => Singleton<GameWorld>.Instance.MainPlayer;

        public void RefreshZones()
        {
            // This method is just lol, but works for this use case.
            _spawnPointMarkers = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<SpawnPointMarker>().ToList();
            
            foreach (var zone in _spawnPointMarkers)
            {
                IterateSpawnPoints(zone);
            }
        }
        private void Awake()
        {
            // If DLSS or FSR are enabled, set a screen scale value
            if (CameraClass.Instance.SSAA.isActiveAndEnabled)
            {
                _screenScale =
                    (float)CameraClass.Instance.SSAA.GetOutputWidth()
                    / (float)CameraClass.Instance.SSAA.GetInputWidth();
                // Plugin.Log.LogDebug($"DLSS or FSR is enabled, scale screen offsets by {_screenScale}");
            }

            RefreshZones();
        }

        private void OnGUI()
        {
            if (guiStyle is null)
            {
                CreateGuiStyle();
            }

            foreach (var spawnPoint in _spawnPointInfo)
            {
                var pos = spawnPoint.Position;
                var dist = Mathf.RoundToInt(
                    (spawnPoint.Position - _player.CameraPosition.position).magnitude
                );

                if (spawnPoint.GUIContent.text.Length <= 0 || !(dist < 300f))
                    continue;

                var screenPos = Camera.main!.WorldToScreenPoint(pos + (Vector3.up * 1.5f));

                // Skip points behind the camera.
                if (screenPos.z <= 0)
                    continue;

                var guiSize = guiStyle.CalcSize(spawnPoint.GUIContent);
                spawnPoint.GUIRect.x = (screenPos.x * _screenScale) - (guiSize.x / 2);
                spawnPoint.GUIRect.y = Screen.height - ((screenPos.y * _screenScale) + guiSize.y);
                spawnPoint.GUIRect.size = guiSize;

                GUI.Box(spawnPoint.GUIRect, spawnPoint.GUIContent, guiStyle);
            }
        }

        private void OnDestroy()
        {
            foreach (var obj in _spawnPointInfo.ToArray())
            {
                _spawnPointInfo.Remove(obj);
            }
        }

        private void IterateSpawnPoints(SpawnPointMarker spawnPoint)
        {
            var spawnPointColor = spawnPoint.SpawnPoint.Sides.Contain(EPlayerSide.Savage) ? Color.red : Color.green;

            CreateSpawnPointInfo(spawnPoint.SpawnPoint, spawnPointColor);
        }

        private void CreateSpawnPointInfo(ISpawnPoint spawnPoint, Color color)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = spawnPoint.Position;
            sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            var sphereRenderer = sphere.GetComponent<Renderer>();
            sphereRenderer.material.color = color;

            var infoText = GetPointInfoText(spawnPoint);

            var pointInfo = new SpawnPointInfo()
            {
                Position = spawnPoint.Position,
                Sphere = sphere,
                GUIContent = new GUIContent() { text = infoText },
                GUIRect = new Rect(),
            };

            _spawnPointInfo.Add(pointInfo);
        }

        private string GetPointInfoText(ISpawnPoint spawnPoint)
        {
            // Make sure we clear the string builder before trying to build a new point.
            _sb.Clear();

            var id = spawnPoint.Id;
            var botZoneName = spawnPoint.BotZoneName;
            var position = spawnPoint.Position;
            var rotation = spawnPoint.Rotation;
            var side = spawnPoint.Sides;
            var categories = spawnPoint.Categories;
            var infiltration = spawnPoint.Infiltration;
            var delayToCanSpawnSec = spawnPoint.DelayToCanSpawnSec;
            var corePointId = spawnPoint.CorePointId;
            var isSniper = spawnPoint.IsSnipeZone;

            if (id != null) AppendLabeledValue("Id", spawnPoint.Id, Color.gray, Color.green);
            if (botZoneName != null) AppendLabeledValue("BotZoneName", spawnPoint.BotZoneName, Color.gray, Color.green);
            if (position != null) AppendLabeledValue("Position", spawnPoint.Position.ToString(), Color.gray, Color.green);
            if (rotation != null) AppendLabeledValue("Rotation", spawnPoint.Rotation.ToString(), Color.gray, Color.green);
            AppendLabeledValue("Side", spawnPoint.Sides.ToString(), Color.gray, Color.green);
            AppendLabeledValue("Categories", spawnPoint.Categories.ToString(), Color.gray, Color.green);
            if (infiltration != null) AppendLabeledValue("Infiltration", spawnPoint.Infiltration.ToString(), Color.gray, Color.green);
            AppendLabeledValue("DelayToCanSpawnSec", spawnPoint.DelayToCanSpawnSec.ToString(), Color.gray, Color.green);
            AppendLabeledValue("CorePointId", spawnPoint.CorePointId.ToString(), Color.gray, Color.green);
            AppendLabeledValue("IsSniper", spawnPoint.IsSnipeZone.ToString(), Color.gray, Color.green);
            AppendLabeledValue("DistanceFromPlayer", Vector3.Distance(_player.Transform.position, spawnPoint.Position).ToString(), Color.gray, Color.green);

            return _sb.ToString();
        }

        private void CreateGuiStyle()
        {
            guiStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                margin = new RectOffset(2, 2, 2, 2),
                richText = true,
            };
        }

        private void AppendLabeledValue(
            string label,
            string data,
            Color labelColor,
            Color dataColor,
            bool labelEnabled = true
        )
        {
            var labelColorString = GetColorString(labelColor);
            var dataColorString = GetColorString(dataColor);

            AppendLabeledValue(label, data, labelColorString, dataColorString, labelEnabled);
        }

        private void AppendLabeledValue(
            string label,
            string data,
            string labelColor,
            string dataColor,
            bool labelEnabled = true
        )
        {
            if (labelEnabled)
            {
                _sb.AppendFormat("<color={0}>{1}:</color>", labelColor, label);
            }

            _sb.AppendFormat(" <color={0}>{1}</color>\n", dataColor, data);
        }

        private static string GetColorString(Color color)
        {
            if (color == Color.black)
                return "black";
            if (color == Color.white)
                return "white";
            if (color == Color.yellow)
                return "yellow";
            if (color == Color.red)
                return "red";
            if (color == Color.green)
                return "green";
            if (color == Color.blue)
                return "blue";
            if (color == Color.cyan)
                return "cyan";
            if (color == Color.magenta)
                return "magenta";
            if (color == Color.gray)
                return "gray";
            if (color == Color.clear)
                return "clear";
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        private class SpawnPointInfo
        {
            public Vector3 Position;
            public GameObject Sphere;
            public GUIContent GUIContent;
            public Rect GUIRect;
        }
    }
}
