using Common.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime {
    /// <summary>
    /// Check for Ground and make sure we adjust ourselfes properly
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMover : MonoBehaviour {
        #region fields

        [Header("Collider Settings")] 
        [InfoBox("<b><u>Step Height Ratio</u></b>\n" +
                 "Increasing the Step height ratio, might lift the character up a bit, the higher the value is.\n" +
                 "Try playing around with the Collider Offset and slightly lift the collider, so it starts a bit above the legs (barely)\n" +
                 "<i>Tip: Observe what happens to the player y position in runtime when changing the Step height ratio and add the difference to the collider offset.</i>")]
        [SerializeField, Range(0f, 1f)] float stepHeightRatio = 0.05f;
        [SerializeField] float colliderHeight = 0.6f;
        [SerializeField] float colliderThickness = 0.27f;
        [SerializeField] Vector3 colliderOffset = new(0f, 0.3f, 0f);

        Rigidbody _rb;
        Transform _tr;
        CapsuleCollider _col;
        RaycastSensor _sensor;
        
        bool _isGrounded;
        float _baseSensorRange;
        Vector3 _currentGroundAdjustmentVelocity;
        int _currentLayer;
        
        [Header("Debug Settings")]
        [SerializeField] bool debugMode;
        bool _usingExtendedSensorRange = true; // Uneven ground look further than base range

        #endregion

        void Awake() {
            Setup();
            RecalculateColliderDimensions();
        }

        void OnValidate() {
            if (gameObject.activeInHierarchy) {
                RecalculateColliderDimensions();
            }
        }

        void Setup() {
            _tr = transform;
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<CapsuleCollider>();
            
            _rb.freezeRotation = true;
            _rb.useGravity = false;
        }
        void RecalibrateSensor() {
            _sensor ??= new RaycastSensor(_tr);
            
            _sensor.SetCastOrigin(_col.bounds.center);
            _sensor.SetCastDirection(RaycastSensor.CastDirection.Down);
            RecalculateSensorLayerMask();
            
            const float safetyDistanceFactor = 0.001f; // To avoid floating point issues
            
            var length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
            _baseSensorRange = length * (1f + safetyDistanceFactor) * _tr.localScale.x;
            _sensor.CastLength = length * _tr.localScale.x;
        }
        void RecalculateSensorLayerMask() {
            var objectLayer = gameObject.layer;
            var layerMask = Physics.AllLayers;
            
            // Ignore the layers that we set in project settings to be ignored
            for (var i = 0; i < 32; i++) {
                if (Physics.GetIgnoreLayerCollision(objectLayer, i)) {
                    layerMask &= ~(1 << i);
                }
            }
            
            var ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer); // Always ignore this layer
            
            _sensor.Layermask = layerMask;
            _currentLayer = objectLayer;
        }
        void RecalculateColliderDimensions() {
            if (_col == null) { // In Editor Mode
                Setup();
            }
            
            _col.height = colliderHeight * (1f - stepHeightRatio);
            _col.radius = colliderThickness / 2f;
            _col.center = colliderOffset * colliderHeight + new Vector3(0, stepHeightRatio * _col.height / 2f, 0);
            
            // TODO: Might need this and resize the collider :/
            // Our Collider is tiny, but normally keeping half the height as radius is a good idea
            if(_col.height / 2f < _col.radius) {
                _col.radius = _col.height / 2f;
            }

            RecalibrateSensor();
        }
        public void CheckForGround() {
            if(_currentLayer != gameObject.layer) {
                RecalculateSensorLayerMask();
            }
            
            // Smoothly move up and down over bumpy terrain
            _currentGroundAdjustmentVelocity = Vector3.zero;
            _sensor.CastLength = _usingExtendedSensorRange 
                ? _baseSensorRange + colliderHeight * _tr.localScale.x * stepHeightRatio
                : _baseSensorRange;
            
            _sensor.Cast();

            _isGrounded = _sensor.HasDetectedHit();
            if (!_isGrounded) return;
            
            var distanceToGround = _sensor.GetDistance();
            // Top boundary of where the player ideally should be positioned
            var upperLimit = colliderHeight * _tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
            var middle = upperLimit + colliderHeight * _tr.localScale.x * stepHeightRatio; // New Player feet position relative to the ground
            var distanceToGo = middle - distanceToGround;
            
            _currentGroundAdjustmentVelocity = _tr.up * (distanceToGo / Time.fixedDeltaTime);
        }
        public bool IsGrounded() => _isGrounded;
        public Vector3 GetGroundNormal() => _sensor.GetNormal();
        public void SetVelocity(Vector3 velocity) => _rb.linearVelocity = velocity + _currentGroundAdjustmentVelocity;
        public void SetExtendedSensorRange(bool extended) => _usingExtendedSensorRange = extended;
    }
}
