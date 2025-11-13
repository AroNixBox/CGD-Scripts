using Core.Runtime.Service.Input;
using Extensions.FSM;
using Gameplay.Runtime.Camera;
using Gameplay.Runtime.Player.Camera;
using Gameplay.Runtime.Player.Combat;
using Gameplay.Runtime.Player.Trajectory;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    public class CombatStanceState : IState {
        readonly PlayerCameraControls _cameraControls;
        // readonly PlayerAnimatorController _animatorController;
        readonly InputReader _inputReader;
        readonly PlayerController _controller;
        
        readonly PlayerWeaponController _weaponController;
        readonly PlayerWeaponStash _weaponStash;
        
        public CombatStanceState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _inputReader = controller.InputReader;
            _controller = controller;
            // _animatorController = controller.AnimatorController;

            _weaponController = controller.WeaponController;
            _weaponStash = controller.WeaponStash;
        }

        public void OnEnter() {
            _inputReader.Fire += Attack;
            _cameraControls.SwitchToControllableCameraMode(PlayerCameraControls.ECameraMode.FirstPerson);
        }

        public void Tick(float deltaTime) {
            SwitchWeapon();
            ChangeProjectileForce();
            AimWeapon();
            _weaponController.PredictTrajectory();
        }

        // TODO: Do via InputAsset
        void ChangeProjectileForce() {
            if(Mouse.current == null)
                return;
            var scrollValueX = Mouse.current.scroll.ReadValue().x;
            
            if (scrollValueX > 0)
                _weaponController.IncreaseProjectileForce();
            else if (scrollValueX < 0)
                _weaponController.DecreaseProjectileForce();
        }

        // TODO: Do via InputAsset
        void SwitchWeapon() {
            if(Mouse.current == null)
                return;
            var scrollValueY = Mouse.current.scroll.ReadValue().y;
            
            if(scrollValueY > 0)
                _weaponStash.SelectWeapon(PlayerWeaponStash.EWeaponIndex.Next);
            else if(scrollValueY < 0)
                _weaponStash.SelectWeapon(PlayerWeaponStash.EWeaponIndex.Previous);
        }


        // Weapon Fwd = Camera Fwd
        void AimWeapon() {
            var firstPersonCamera = _cameraControls.GetActiveCameraTransform();
            var activeCameraForward = firstPersonCamera.forward;
            var spawnedWeapon = _weaponStash.GetSpawnedWeapon();
            spawnedWeapon.transform.forward = activeCameraForward;
        }
        void Attack() {
            // TODO:
            // If we wanna trigger an Animation
            // _animatorController.ChangeAnimationState(AnimationParameters.CastSpell);
            
            // When the Projectile expires, we reset the Bullet Cam again and give Priority to the next player
            var projectile = _weaponController.FireWeapon(EndTurn);
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
            _inputReader.Fire -= Attack;
            _cameraControls.ResetControllableCameras();
            _weaponController.ResetProjectileForce();
            
            // Remove Trajectory Line
            if (TrajectoryPredictor.Instance == null) {
                Debug.LogError("No TrajectoryPredictor in Scene");
            }
            else {
                TrajectoryPredictor.Instance.RemoveTrajectoryLine();
            }
        }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}