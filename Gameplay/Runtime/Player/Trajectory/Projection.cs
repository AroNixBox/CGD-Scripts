using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Gameplay.Runtime.Player.Trajectory {
    public class Projection : MonoBehaviour {
        [SerializeField] Transform environment;
        [SerializeField] LineRenderer trajectoryLine;
        [SerializeField] int trajectoryPhysicsIterations;

        [SerializeField] int poolDefaultCapacity;
        [SerializeField] int poolMaxSize;

        IObjectPool<Projectile> _pool;
        
        Scene _simulationScene;
        PhysicsScene _physicsScene;

        readonly Dictionary<Transform, Transform> _realToPhysicsMapping = new();

        public void InitializePool(Projectile projectilePrefab) {
            _pool = new ObjectPool<Projectile>(
                createFunc: () => CreateItem(projectilePrefab),
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: Destroy,
                collectionCheck: true,
                defaultCapacity: poolDefaultCapacity,
                maxSize: poolMaxSize
            );
        }

        void OnGet(Projectile projectile) {
            projectile.gameObject.SetActive(true);
        }

        void OnRelease(Projectile projectile) {
            projectile.gameObject.SetActive(false);
            projectile.ResetRigidbody();
        }

        // Creates a new pooled GameObject the first time (and whenever the pool needs more).
        Projectile CreateItem(Projectile prefab) {
            var objClone = Instantiate(prefab);
            objClone.GetComponent<MeshRenderer>().enabled = false; // Disable visuals
            SceneManager.MoveGameObjectToScene(objClone.gameObject, _simulationScene);
            // Disable completely
            objClone.gameObject.SetActive(false);
            return objClone;
        }
        
        void Start() {
            CreatePhysicsScene();
        }

        // TODO: Do this event based so each object only moves its correspond if its moved
        void Update() {
            foreach (var item in _realToPhysicsMapping) {
                item.Value.transform.position = item.Key.transform.position;
            }
        }

        // Update is called once per frame
        void CreatePhysicsScene() {
            _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            _physicsScene = _simulationScene.GetPhysicsScene();

            CloneRecursivelyIntoSimulationScene(environment);
        }

        void CloneRecursivelyIntoSimulationScene(Transform source) {
            // Only clone if its a visible object
            if (source.GetComponent<MeshRenderer>()) {
                CloneObjectIntoSimulationScene();
            }

            // Go through children anyways
            foreach (Transform tr in source) {
                CloneRecursivelyIntoSimulationScene(tr);
            }

            return;
            
            void CloneObjectIntoSimulationScene() {
                var objClone = Instantiate(source, source.position, source.rotation);
                objClone.GetComponent<MeshRenderer>().enabled = false; // Disable visuals
                SceneManager.MoveGameObjectToScene(objClone.gameObject, _simulationScene);
            
                if(objClone.gameObject.isStatic) { return; }
            
                // Only add dynamic Objects
                if (!_realToPhysicsMapping.TryAdd(source, objClone)) {
                    Debug.LogError($"Couldnt add {source.name} and its clone {objClone.name} to the Mapping.");
                }
            }
        }

        public void SimulateTrajectory(Vector3 pos, Vector3 velocity) {
            var projectileClone = _pool.Get();
            projectileClone.GetComponent<Projectile>().Init(pos, velocity);

            trajectoryLine.positionCount = trajectoryPhysicsIterations;
            for(var i = 0; i < trajectoryPhysicsIterations; i++) {
                _physicsScene.Simulate(Time.fixedDeltaTime);
                trajectoryLine.SetPosition(i, projectileClone.transform.position);
            }
            
            _pool.Release(projectileClone);
        }
    }
}
