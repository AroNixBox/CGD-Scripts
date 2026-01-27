using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Gameplay.Runtime {
    public class TerrainHeightWriter : MonoBehaviour {
        [SerializeField] private bool updateOnUpdate = false;
        [SerializeField] private List<Transform> positionsTar;
        [SerializeField] private List<Transform> positionsLava;

        [SerializeField] private int resolution = 348;
        [SerializeField] private float worldSize = 128f;

        [SerializeField, Required] private Material heightWriteMaterial;
        [SerializeField, Required] private Material terrainMaterial;

        private RenderTexture heightTar;
        private RenderTexture heightLava;
        private readonly int seedTar = 0;
        private readonly int seedLava = 69;

        private void Start() {
            heightTar = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R16) { enableRandomWrite = false };
            heightTar.Create();
            heightLava = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R16) { enableRandomWrite = false };
            heightLava.Create();

            terrainMaterial.SetTexture("_HeightTexTar", heightTar);
            terrainMaterial.SetTexture("_HeightTexLava", heightLava);

            ChangeTargets(HazardType.Tar, null);
            ChangeTargets(HazardType.Lava, null);
        }

        //remove, once functionalities are final
        private void Update() {
            if (updateOnUpdate) {
                ChangeTargets(HazardType.Tar, null);
                ChangeTargets(HazardType.Lava, null);
            }
        }

        //call this to refresh one of the height maps
        public void RefreshTexture(HazardType type) {
            if (type == HazardType.Tar) {
                ReRender(ref heightTar, ref positionsTar, seedTar);
            } else if (type == HazardType.Lava) {
                ReRender(ref heightLava, ref positionsLava, seedLava);
            } else {
                Debug.LogError("Mode " + type + " unknown!");
            }
        }

        //rerenders one of the texture maps
        private void ReRender(ref RenderTexture map, ref List<Transform> positions, int seed) {
            Graphics.SetRenderTarget(map);
            GL.Clear(false, true, Color.black);

            heightWriteMaterial.SetFloat("_WorldSize", worldSize);
            heightWriteMaterial.SetFloat("_Seed", seed);

            foreach (Transform t in positions) {
                heightWriteMaterial.SetVector("_Center", t.position);
                heightWriteMaterial.SetFloat("_DistanceMultiplier", (t.localScale.x / 1.05f) / 5);
                Graphics.Blit(null, map, heightWriteMaterial);
            }

            Graphics.SetRenderTarget(null);
        }

        //add or delete a position entry from a table depending on mode
        public void ChangeTargets(HazardType type, Transform pos) {
            if (type == HazardType.Tar) {
                CheckTable(ref positionsTar, pos);
            } else if (type == HazardType.Lava) {
                CheckTable(ref positionsLava, pos);
            } else {
                Debug.LogError("Mode " + type + " unknown!");
            }

            RefreshTexture(type);
        }

        //add or delete a position entry from a table
        private void CheckTable(ref List<Transform> list, Transform pos) {
            if (pos == null) {
                list.RemoveAll(t => t == null);
            } else if (list.Contains(pos)) {
                list.Remove(pos);
            } else {
                list.Add(pos);
            }
        }
    }
}