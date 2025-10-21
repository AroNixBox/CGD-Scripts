using Core.Runtime.Authority;
using Core.Runtime.Service.Input;
using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

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
            _inputReader.Fire += _playerController.AuthorityEntity.GiveNextAuthority; // TODO: Call from Spell, but remember that we dont wanna go in no auth state immediatly
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.FirstPerson);
        }

        public void Tick() { }
        
        void Attack() {
            _animatorController.ChangeAnimationState(AnimationParameters.CastSpell);
        }

        public void OnExit() {
            _inputReader.Fire -= Attack;
            _inputReader.Fire -= _playerController.AuthorityEntity.GiveNextAuthority;
        }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}