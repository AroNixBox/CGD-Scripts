using Core.Runtime.Service.Input;
using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    public class CombatStanceState : IState {
        readonly PlayerCameraControls _cameraControls;
        readonly PlayerAnimatorController _animatorController;
        readonly InputReader _inputReader;
        readonly PlayerController _playerController;
        public CombatStanceState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _inputReader = controller.InputReader;
            _playerController = controller;
            _animatorController = controller.AnimatorController;
        }

        public void OnEnter() {
            _inputReader.Fire += Attack;
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.FirstPerson);
        }

        public void Tick(float deltaTime) { }        
        void Attack() {
            _animatorController.ChangeAnimationState(AnimationParameters.CastSpell);
            _playerController.AuthorityEntity.ResetAuthority();
            _ = NextPlayer();
        }

        // TODO: Switching to the next player should not be time based, but rather called from the ability we cast
        async UniTask NextPlayer() {
            await UniTask.Delay(5000); // Placeholder for spell cast duration
            _playerController.AuthorityEntity.GiveNextAuthority();
        }

        public void OnExit() {
            _inputReader.Fire -= Attack;
        }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}