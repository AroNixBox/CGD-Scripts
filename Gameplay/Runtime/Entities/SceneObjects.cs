using UnityEngine;
using Sirenix.OdinInspector;

namespace Gameplay.Runtime {
    public class SceneObjects : MonoBehaviour {
        [SerializeField, Required] private Transform hazards;
        [SerializeField, Required] private TerrainHeightWriter offsetTerrain;

        public Transform Hazards => hazards;
        public TerrainHeightWriter OffsetTerrain => offsetTerrain;
    }
}
