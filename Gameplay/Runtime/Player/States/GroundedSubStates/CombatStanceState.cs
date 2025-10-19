using Core.Runtime.Authority;
using Core.Runtime.Service.Input;
using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime {
    public class CombatStanceState : IState {
        readonly PlayerCameraControls _cameraControls;
        readonly InputReader _inputReader;
        readonly Transform _player;
        readonly PlayerController _playerController;
        public CombatStanceState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _player = controller.transform;
            _inputReader = controller.InputReader;
            _playerController = controller;
        }

        public void OnEnter() {
            _inputReader.Fire += TriggerShockwave;
            _inputReader.Fire += _playerController.AuthorityEntity.GiveNextAuthority;
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.FirstPerson);
        }

        public void Tick() { }
        
        void TriggerShockwave() {
            var hitColliders = new Collider[100];
            const float explosionRadius = 10f;
            var size = Physics.OverlapSphereNonAlloc(_player.position, explosionRadius, hitColliders);

            for(var i = 0; i < size; i++) {
                var rb = hitColliders[i].attachedRigidbody;

                if (rb == null || rb.transform.IsChildOf(_player) || rb.transform == _player) {
                    continue;
                }

                var entity = rb.GetComponent<AuthorityEntity>();
                
                const float explosionForce = 150;
                const float upwardsModifier = 1f;
                const float forceRadius = explosionRadius * .5f;
                
                if (entity == null) {
                    rb.AddExplosionForce(explosionForce * .25f, _player.position, forceRadius, upwardsModifier,
                        ForceMode.Impulse);
                    continue;
                }
                
                rb.AddExplosionForce(explosionForce, _player.position, forceRadius, upwardsModifier,
                    ForceMode.Impulse);
            }
        }

        public void OnExit() {
            _inputReader.Fire -= TriggerShockwave;
            _inputReader.Fire -= _playerController.AuthorityEntity.GiveNextAuthority;
        }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}