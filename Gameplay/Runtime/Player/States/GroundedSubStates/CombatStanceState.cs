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
        TrajectoryPredictor _trajectoryPredictor;
        
        public CombatStanceState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _inputReader = controller.InputReader;
            _controller = controller;
            // _animatorController = controller.AnimatorController;
            _weaponStash = controller.WeaponStash;
        }

        public void OnEnter() {
            _inputReader.Fire += Attack;
            _inputReader.NextGun += _weaponStash.SelectNextWeapon;
            _inputReader.PreviousGun += _weaponStash.SelectPreviousWeapon;
            
            _cameraControls.SwitchToControllableCameraMode(PlayerCameraControls.ECameraMode.FirstPerson).ContinueWith(
                    () => _controller.VisualModel.gameObject.SetActive(false)
                ).Forget();
            _weaponStash.SpawnSelectedWeapon();
        }

        public void Tick(float deltaTime) {
            var currentWeapon = _weaponStash.GetSpawnedWeapon();
            if(currentWeapon != null)
                ChangeProjectileForce(currentWeapon);
            AimWeapon();
            if(currentWeapon != null)
                currentWeapon.PredictTrajectory();
        }

        void ChangeProjectileForce(Weapon currentWeapon) { 
            if (_inputReader.IsWeaponForceIncreasing)
                currentWeapon.IncreaseProjectileForce();
            else if (_inputReader.IsWeaponForceDecreasing)
                currentWeapon.DecreaseProjectileForce();
        }

        // Weapon Fwd = Camera Fwd
        void AimWeapon() {
            // TODO: Move WeaponSlot with the rotation 
            var firstPersonCamera = _cameraControls.GetActiveCameraTransform();
            var activeCameraForward = firstPersonCamera.forward;
            var spawnedWeapon = _weaponStash.GetSpawnedWeapon();
            spawnedWeapon.transform.forward = activeCameraForward;
        }
        void Attack() {
            // TODO:
            // If we wanna trigger an Animation
            // _animatorController.ChangeAnimationState(AnimationParameters.CastSpell);

            var currentWeapon = _weaponStash.GetSpawnedWeapon();
            // When the Projectile expires, we reset the Bullet Cam again and give Priority to the next player
            var projectile = currentWeapon.FireWeapon(EndTurn);
            _cameraControls.EnableBulletCamera(projectile.transform);
            
            // Exit Condition
            _controller.AuthorityEntity.ResetAuthority();
        }

        void EndTurn() {
            _cameraControls.ResetBulletCamera();
            // Entry Condition for next Player Substatemachine
            _controller.AuthorityEntity.GiveNextAuthority();
        }

        public void OnExit() {
            _inputReader.NextGun -= _weaponStash.SelectNextWeapon;
            _inputReader.PreviousGun -= _weaponStash.SelectPreviousWeapon;
            _inputReader.Fire -= Attack;
            _cameraControls.ResetControllableCameras();
            _weaponStash.DespawnSelectedWeapon();
            _controller.VisualModel.gameObject.SetActive(true);

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