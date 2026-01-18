using System;
using Core.Runtime.Service;
using Core.Runtime.Service.Input;
using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using Gameplay.Runtime.Player.Combat;
using Gameplay.Runtime.Player.Trajectory;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    public class CombatStanceState : IState {
        readonly PlayerCameraControls _cameraControls;
        // readonly PlayerAnimatorController _animatorController;
        readonly InputReader _inputReader;
        readonly PlayerController _controller;
        readonly PlayerWeaponStash _weaponStash;
        readonly Action<Projectile> _onProjectileFired;
        TrajectoryPredictor _trajectoryPredictor;
        
        public CombatStanceState(PlayerController controller, Action<Projectile> onProjectileFired) {
            _cameraControls = controller.PlayerCameraControls;
            _inputReader = controller.InputReader;
            _controller = controller;
            // _animatorController = controller.AnimatorController;
            _weaponStash = controller.WeaponStash;
            _onProjectileFired = onProjectileFired;
        }

        public void OnEnter() {
            _inputReader.Fire += Attack;
            _inputReader.Move += _weaponStash.SelectWeapon;
            
            _cameraControls.SwitchToControllableCameraMode(PlayerCameraControls.ECameraMode.FirstPerson).ContinueWith(
                    () => _controller.VisualModel.gameObject.SetActive(false)
                ).Forget();
            _weaponStash.SelectCurrentWeapon();
            _controller.OnCombatStanceStateEntered.Invoke();
            _weaponStash.ResetShootingPower();
        }

        public void Tick(float deltaTime) {
            ChangeProjectileForce();
            AimWeapon();
            _weaponStash.PredictTrajectory();
        }

        void ChangeProjectileForce() { 
            if (_inputReader.IsWeaponForceIncreasing)
                _weaponStash.IncreaseProjectileForce();
            else if (_inputReader.IsWeaponForceDecreasing)
                _weaponStash.DecreaseProjectileForce();
        }
        
        

        // Weapon Fwd = Camera Fwd
        void AimWeapon() {
            var spawnedWeapon = _weaponStash.GetSpawnedWeapon();
            if (spawnedWeapon == null) return;

            var socket = spawnedWeapon.transform.parent;
            if (!socket.TryGetComponent(out WeaponSway sway)) {
                sway = socket.gameObject.AddComponent<WeaponSway>();
            }

            sway.ProcessSway(_inputReader.LastLookDirection, _inputReader.IsCurrentDeviceMouse);
        }
        void Attack() {
            if (!_weaponStash.TryFire(out var projectile)) {
                // No Ammo
                return;
            }

            _controller.AuthorityEntity.ResetAuthority();
            _onProjectileFired?.Invoke(projectile);
        }


        public void OnExit() {
            _inputReader.Move -= _weaponStash.SelectWeapon;
            _inputReader.Fire -= Attack;
            _cameraControls.ResetControllableCameras();
            _weaponStash.DespawnSelectedWeapon();
            _controller.VisualModel.gameObject.SetActive(true);
            _controller.OnCombatStanceStateExited.Invoke();

            // Remove Trajectory Line
            if(_trajectoryPredictor == null)
                if (!ServiceLocator.TryGet(out _trajectoryPredictor))
                    throw new NullReferenceException("Trajectory Projector not available via Service Locator");
            _trajectoryPredictor.RemoveTrajectoryLine();
        }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}