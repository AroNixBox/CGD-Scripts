using Core.Runtime.Service.Input;
using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using Gameplay.Runtime.Player.Combat;
using Gameplay.Runtime.Player.Trajectory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime {
    public class CombatStanceState : IState {
        readonly PlayerCameraControls _cameraControls;
        readonly PlayerAnimatorController _animatorController;
        readonly InputReader _inputReader;
        readonly PlayerController _playerController;

        readonly PlayerWeaponController _weaponController;
        readonly PlayerWeaponStash _weaponStash;
        public CombatStanceState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _inputReader = controller.InputReader;
            _playerController = controller;
            _animatorController = controller.AnimatorController;

            _weaponController = controller.WeaponController;
            _weaponStash = controller.WeaponStash;
        }

        public void OnEnter() {
            _inputReader.Fire += Attack;
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.FirstPerson);
        }

        public void Tick(float deltaTime) {
            AimWeapon();
            PredictTrajectory();
            
            if(Mouse.current == null)
                return;
                    
            // TODO: Do via InputAsset
            var scrollValue = Mouse.current.scroll.ReadValue();
            if(scrollValue.y > 0)
                _weaponStash.SelectWeapon(PlayerWeaponStash.EWeaponIndex.Next);
            else if(scrollValue.y < 0)
                _weaponStash.SelectWeapon(PlayerWeaponStash.EWeaponIndex.Previous);
        }

        
        // Weapon Fwd = Camera Fwd
        void AimWeapon() {
            var firstPersonCamera = _cameraControls.GetActiveCameraTransform();
            var activeCameraForward = firstPersonCamera.forward;
            var spawnedWeapon = _weaponStash.GetSpawnedWeapon();
            spawnedWeapon.transform.forward = activeCameraForward;
        }

        // TODO: Right place? SRP?
        void PredictTrajectory() {
            if(TrajectoryPredictor.Instance == null)
                Debug.LogError("No TrajectoryPredictor in Scene");

            var currentWeaponData = _weaponStash.GetCurrentWeaponData();
            var currentProjectileData = currentWeaponData.ProjectileData;
            var spawnedWeaponData = _weaponStash.GetSpawnedWeapon();
            TrajectoryPredictor.Instance.PredictTrajectory(spawnedWeaponData.GetWeaponProperties(),
                currentProjectileData.GetProjectileProperties());
        }

        void Attack() {
            // TODO:
            // If we wanna trigger an Animation
            // _animatorController.ChangeAnimationState(AnimationParameters.CastSpell);

            _weaponController.FireWeapon();
            _playerController.AuthorityEntity.ResetAuthority();
            _ = NextPlayer();
        }

        // TODO: This state should END the moment the player shoots, the timer should be in the CombatRecoveryState...
        // TODO: Switching to the next player should not be time based, but rather called from the ability we cast
        async UniTask NextPlayer() {
            await UniTask.Delay(5000); // Placeholder for spell cast duration
            _playerController.AuthorityEntity.GiveNextAuthority();
        }

        public void OnExit() {
            // Remove Trajectory Line
            if(TrajectoryPredictor.Instance == null)
                Debug.LogError("No TrajectoryPredictor in Scene");
            TrajectoryPredictor.Instance.RemoveTrajectoryLine();
            
            _inputReader.Fire -= Attack;
        }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}