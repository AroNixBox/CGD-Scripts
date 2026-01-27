using Sirenix.OdinInspector;
using Gameplay.Runtime;
using UnityEngine;
using System.Linq;

public enum HazardType {
    Tar,
    Lava
}

public class HazardSpawner : MonoBehaviour {
    [SerializeField, Required] private HazardType hazardType;
    [SerializeField] private GameObject hazarProxy;
    [SerializeField] private float puddleRadius = 10f;

    public void GeneratePuddle(Vector3 hitPoint) {
        var staticParent = GameObject.Find("Static");
        var terrainWriter = staticParent.GetComponent<SceneObjects>().OffsetTerrain.GetComponent<TerrainHeightWriter>();

        var proxy = Instantiate(hazarProxy, hitPoint, Quaternion.identity, staticParent.GetComponent<SceneObjects>().Hazards);
        proxy.transform.localScale = new(puddleRadius * 1.05f, puddleRadius * 1.05f, puddleRadius * 1.05f);

        Hazard hazard = proxy.GetComponents<MonoBehaviour>().OfType<Hazard>().FirstOrDefault();
        hazard.TerrainHazardManager = terrainWriter;
        hazard.HazardType = hazardType;

        terrainWriter.ChangeTargets(hazardType, proxy.transform);
    }
}
