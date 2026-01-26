using UnityEngine;
using Sirenix.OdinInspector;

namespace Gameplay.Runtime {
    public class SceneObjects : MonoBehaviour {
        [SerializeField, Required] private Transform tars;
        [SerializeField, Required] private Transform lavas;
        [SerializeField, Required] private TerrainHeightWriter offsetTerrain;

        public Transform Tars => tars;
        public Transform Lavas => lavas;
        public TerrainHeightWriter OffsetTerrain => offsetTerrain;
    }
}
