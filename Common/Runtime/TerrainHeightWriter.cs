using UnityEngine;

public class TerrainHeightWriter : MonoBehaviour {
    [SerializeField] private Transform[] positions;

    [SerializeField] private int resolution = 348;
    [SerializeField] private float worldSize = 128f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float strength = 1f;

    [SerializeField] private Material heightWriteMaterial;
    [SerializeField] private Material terrainMaterial;

    private RenderTexture heightRT;

    private void Start() {
        heightRT = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R16) {
            enableRandomWrite = false
        };
        heightRT.Create();

        terrainMaterial.SetTexture("_HeightTex", heightRT);
    }

    //exchange with on demand function so it wont be called every frame, once functionalities are final
    private void Update() {
        Graphics.SetRenderTarget(heightRT);
        GL.Clear(false, true, Color.black);

        heightWriteMaterial.SetFloat("_Radius", radius);
        heightWriteMaterial.SetFloat("_Strength", strength);
        heightWriteMaterial.SetFloat("_WorldSize", worldSize);

        foreach (Transform t in positions) {
            heightWriteMaterial.SetVector("_Center", t.position);
            Graphics.Blit(null, heightRT, heightWriteMaterial);
        }

        Graphics.SetRenderTarget(null);
    }
}
