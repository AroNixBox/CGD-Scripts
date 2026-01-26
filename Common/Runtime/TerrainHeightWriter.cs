using System.Collections.Generic;
using UnityEngine;

public class TerrainHeightWriter : MonoBehaviour {
    [SerializeField] private List<Transform> positionsTar;
    [SerializeField] private List<Transform> positionsLava;

    [SerializeField] private int resolution = 348;
    [SerializeField] private float worldSize = 128f;

    [SerializeField] private Material heightWriteMaterial;
    [SerializeField] private Material terrainMaterial;

    private RenderTexture heightTar;
    private RenderTexture heightLava;
    private int seedTar = 0;
    private int seedLava = 69;

    private void Start() {
        heightTar = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R16) {enableRandomWrite = false};
        heightTar.Create();
        heightLava = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R16) {enableRandomWrite = false};
        heightLava.Create();

        terrainMaterial.SetTexture("_HeightTexTar", heightTar);
        terrainMaterial.SetTexture("_HeightTexLava", heightLava);
    }

    //remove, once functionalities are final
    private void Update() {
        RefreshTexture("tar");
        RefreshTexture("lava");
    }

    public void RefreshTexture(string mode) {
        ChangeTargets(mode, null);

        if (mode == "tar") {
            ReRender(ref heightTar, ref positionsTar, seedTar);
        } else if (mode == "lava") {
            ReRender(ref heightLava, ref positionsLava, seedLava);
        } else {
            Debug.LogError("Mode " + mode + " unknown!");
        }
    }

    private void ReRender(ref RenderTexture map, ref List<Transform> positions, int seed) {
        Graphics.SetRenderTarget(map);
        GL.Clear(false, true, Color.black);

        heightWriteMaterial.SetFloat("_WorldSize", worldSize);
        heightWriteMaterial.SetFloat("_Seed", seed);

        foreach (Transform t in positions) {
            heightWriteMaterial.SetVector("_Center", t.position);
            Graphics.Blit(null, map, heightWriteMaterial);
        }

        Graphics.SetRenderTarget(null);
    }

    public void ChangeTargets(string mode, Transform pos) {
        if (mode == "tar") {
            CheckTable(ref positionsTar, pos);
        } else if (mode == "lava") {
            CheckTable(ref positionsLava, pos);
        } else {
            Debug.LogError("Mode " + mode + " unknown!");
        }
    }

    private void CheckTable(ref List<Transform> list, Transform pos) {
        if (pos == null) {
            list.RemoveAll(t => t == null);
        } else {
            list.Add(pos);
        }
    }
}
