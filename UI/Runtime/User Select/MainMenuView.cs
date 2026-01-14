using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime {
    public class MainMenuView : MonoBehaviour {
        [SerializeField, Required] Button startButton;

        public event Action OnStartClicked = delegate { };

        void OnEnable() {
            startButton.onClick.AddListener(StartClicked);;
        }

        void StartClicked() => OnStartClicked.Invoke();
        
        void OnDisable() {
            startButton.onClick.RemoveListener(StartClicked);;
        }
    }
}
