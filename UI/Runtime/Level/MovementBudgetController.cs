using System;
using Gameplay.Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class MovementBudgetController : MonoBehaviour {
        [SerializeField, Required] MovementBudgetView view;
        [SerializeField, Required] PlayerController playerController;

        void OnEnable() {
            if (playerController == null) throw new NullReferenceException("Wont show Movement Budget UI due to missing player reference");
            
            playerController.OnMovementBudgetChanged += HandleBudgetChanged;
            playerController.OnLocomotionStateEntered += view.Show;
            playerController.OnLocomotionStateExited += view.Hide;
        }

        void Start() => view.Hide();

        void OnDisable() {
            if (playerController == null) return;
            
            playerController.OnMovementBudgetChanged -= HandleBudgetChanged;
            playerController.OnLocomotionStateEntered -= view.Show;
            playerController.OnLocomotionStateExited -= view.Hide;
        }

        void HandleBudgetChanged(float currentTime, float maxTime) {
            view.UpdateBudget(currentTime, maxTime);
        }
    }
}

