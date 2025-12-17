using System.Linq;

namespace acidphantasm_botplacementsystem.Utils;

using UnityEngine;

public class BoundsVisualizer : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.3f;

    void Start()
    {
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Unlit/Color"));
            BoundsVisualizer[] zones = FindObjectsOfType<BoundsVisualizer>();
            int index = System.Array.IndexOf(zones, this);
            float hue = (float)index / zones.Length;
            lineMaterial.color = Color.HSVToRGB(hue, 0.8f, 0.9f);
        }

        var childrenColliders = GetComponentsInChildren<BoxCollider>().Where(c => c.gameObject != gameObject).ToArray();
        foreach (var collider in childrenColliders)
        {
            CreateAndSetRenderer(collider);
            CreateLabel(collider, transform.gameObject.name);
        }
    }
    
    void CreateAndSetRenderer(BoxCollider col)
    {
        LineRenderer lr = col.gameObject.AddComponent<LineRenderer>();

        lr.material = lineMaterial;
        lr.widthMultiplier = lineWidth;
        lr.loop = false;
        lr.useWorldSpace = true;

        Vector3 center = col.transform.TransformPoint(col.center);
        Vector3 ext = Vector3.Scale(col.size * 0.5f, col.transform.lossyScale);

        Vector3[] corners =
        {
            center + new Vector3( ext.x,  ext.y,  ext.z),
            center + new Vector3(-ext.x,  ext.y,  ext.z),
            center + new Vector3(-ext.x, -ext.y,  ext.z),
            center + new Vector3( ext.x, -ext.y,  ext.z),

            center + new Vector3( ext.x,  ext.y, -ext.z),
            center + new Vector3(-ext.x,  ext.y, -ext.z),
            center + new Vector3(-ext.x, -ext.y, -ext.z),
            center + new Vector3( ext.x, -ext.y, -ext.z),
        };

        Vector3[] pts = new Vector3[]
        {
            corners[0], corners[1], corners[2], corners[3], corners[0],
            corners[4], corners[5], corners[6], corners[7], corners[4],
            corners[0], corners[4],
            corners[1], corners[5],
            corners[2], corners[6],
            corners[3], corners[7]
        };

        lr.positionCount = pts.Length;
        lr.SetPositions(pts);
    }
    
    void CreateLabel(BoxCollider col, string text)
    {
        GameObject labelObj = new GameObject("ZoneLabel");
        labelObj.transform.SetParent(col.transform, false);

        // Position above collider
        Vector3 topCenter = col.center + new Vector3(0, col.size.y / 2 + 0.2f, 0);
        labelObj.transform.localPosition = topCenter;
        
        
        labelObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        TextMesh tm = labelObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 48;
        tm.color = Color.yellow;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.LowerCenter;
    }
}
