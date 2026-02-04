﻿using Core.Runtime.Authority;
using Core.Runtime.Service;
using Core.Runtime.Service.Input;
using Gameplay.Runtime.Player;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Runtime.Menus {
    public class PauseMenuController : MonoBehaviour {
        [SerializeField, Required] PauseMenuView view;
        [SerializeField, Required] InputReader inputReader;
        [SerializeField, Required] GameOverMenuView gameOverMenuView;
        
        bool _isInCombatStance;
        PlayerController _activePlayerController;
        AuthorityManager _authorityManager;

        void OnEnable() {
            view.BindButtons(
                onResume: ResumeGame,
                onRestart: () => SceneManager.LoadScene(SceneManager.GetActiveScene().name),
                onSettings: OpenSettings,
                onHelp: OpenHelp,
                onMenu: () => SceneManager.LoadScene("Scenes/User Hub"),
                onQuit: QuitGame
            );
            view.Hide();
        }

        void Start() {
            inputReader.StopCombat += OnEscapePressed;
            
            // Subscribe to player authority changes to track combat stance
            if (ServiceLocator.TryGet(out _authorityManager)) {
                _authorityManager.OnEntityAuthorityGained += OnEntityAuthorityGained;
            }
        }

        void OnEntityAuthorityGained(AuthorityEntity entity) {
            if (entity == null) return;
            
            var playerController = entity.GetComponent<PlayerController>();
            if (playerController != null && playerController != _activePlayerController) {
                UnsubscribeFromPlayerController();
                SubscribeToPlayerController(playerController);
            }
        }

        void SubscribeToPlayerController(PlayerController controller) {
            _activePlayerController = controller;
            controller.OnCombatStanceStateEntered += OnCombatStanceEntered;
            controller.OnCombatStanceStateExited += OnCombatStanceExited;
        }

        void UnsubscribeFromPlayerController() {
            if (_activePlayerController == null) return;
            
            _activePlayerController.OnCombatStanceStateEntered -= OnCombatStanceEntered;
            _activePlayerController.OnCombatStanceStateExited -= OnCombatStanceExited;
            _activePlayerController = null;
        }

        void OnCombatStanceEntered() {
            _isInCombatStance = true;
        }

        void OnCombatStanceExited() {
            _isInCombatStance = false;
        }

        void OnEscapePressed() {
            // Don't open pause menu while in combat stance (ESC is used to exit combat)
            if (_isInCombatStance) return;
            
            TogglePauseMenu();
        }

        void TogglePauseMenu() {
            // Don't open pause menu while game over menu is visible
            if (gameOverMenuView != null && gameOverMenuView.IsVisible) return;
            
            if (view.IsVisible) {
                ResumeGame();
            } else {
                PauseGame();
            }
        }

        void PauseGame() {
            inputReader.DisableActionMap(InputReader.ActionMapName.Player);
            view.Show();
        }

        void ResumeGame() {
            view.Hide();
            inputReader.EnableActionMap(InputReader.ActionMapName.Player);
        }

        void OpenSettings() {
            throw new NotImplementedException();
        }

        void OpenHelp() {
            throw new NotImplementedException();
        }

        void QuitGame() {
            Application.Quit();
        }

        void OnDisable() {
            view.UnbindButtons(
                onResume: ResumeGame,
                onSettings: OpenSettings,
                onHelp: OpenHelp,
                onQuit: QuitGame
            );
        }

        void OnDestroy() {
            inputReader.StopCombat -= OnEscapePressed;
            UnsubscribeFromPlayerController();
            
            if (_authorityManager != null) {
                _authorityManager.OnEntityAuthorityGained -= OnEntityAuthorityGained;
            }
        }
    }
}

