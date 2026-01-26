using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightWriter : MonoBehaviour {
    [SerializeField] private List<Transform> positions;

    [SerializeField] private int resolution = 348;
    [SerializeField] private float worldSize = 128f;

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

    //remove, once functionalities are final
    private void Update() {
        RefreshTexture();
    }

    public void RefreshTexture() {
        ChangeTargets(null);

        Graphics.SetRenderTarget(heightRT);
        GL.Clear(false, true, Color.black);

        heightWriteMaterial.SetFloat("_WorldSize", worldSize);

        foreach (Transform t in positions) {
            heightWriteMaterial.SetVector("_Center", t.position);
            Graphics.Blit(null, heightRT, heightWriteMaterial);
        }

        Graphics.SetRenderTarget(null);
    }

    public void ChangeTargets(Transform pos) {
        if (pos == null) {
            positions.RemoveAll(t => t == null);
        } else {
            positions.Add(pos);
        }
    }
}
