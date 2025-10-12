using System.Collections.Generic;
using Common.Runtime.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Core.Runtime.Service.Input {
    // Interface for input reading. Use this when you want to create different input readers
    public interface IInputReader {
        void EnablePlayerActions();
    }

    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/Reader")]
    public class InputReader : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IUIActions, IInputReader {
        [SerializeField] ActionMapName initialActionMap = ActionMapName.Player;
        // The actual input actions asset. This will be initialized in EnablePlayerActions
        public InputSystem_Actions InputActions { get; private set; }
        ActionMapName _currentActionMap;
        readonly Dictionary<ActionMapName, InputActionMap> _actionMaps = new();
        
        #region Player Map Shared Data

        public event UnityAction<Vector2> Move = delegate { };
        public Vector2 MoveDirection => InputActions.Player.Move.ReadValue<Vector2>();
        
        #endregion
        
        #region UI Map Shared Data
        
        /// <summary> Tracks the position of the "look" - point input and if our Input device is a mouse </summary>
        public event UnityAction<Vector2, bool> PointUI = delegate { };
        /// <summary> Transports if we hit the UI with that click or not, can use if needed </summary>
        public event UnityAction<bool> ClickUI = delegate { };
        /// <summary>Transports the scroll wheel value </summary>
        
        
        public event UnityAction<Vector2> ScrollWheelUI = delegate { };
        public event UnityAction<Vector2, bool> NavigateUI = delegate { };
        public event UnityAction SubmitUI = delegate { };
        public event UnityAction MiddleClickUI = delegate { };
        public event UnityAction RightClickUI = delegate { };
        public event UnityAction CancelUI = delegate { };
        
        #endregion
        
        #region Player Map Methods
        public void OnMove(InputAction.CallbackContext context) {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        #endregion

        #region UI Map Methods

        public void OnNavigate(InputAction.CallbackContext context) {
            NavigateUI.Invoke(context.ReadValue<Vector2>(), InputUtils.IsDeviceMouse(context));
        }

        public void OnSubmit(InputAction.CallbackContext context) {
            if(context.phase != InputActionPhase.Performed) { return; }
            
            SubmitUI.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context) {
            if(context.phase != InputActionPhase.Performed) { return; }
            
            CancelUI.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context) {
            // We are not only tracking the active state if moving the mouse, but always
            PointUI.Invoke(context.ReadValue<Vector2>(), InputUtils.IsDeviceMouse(context));
        }

        public void OnClick(InputAction.CallbackContext context) {
            if(context.phase != InputActionPhase.Performed) { return; }
            
            ClickUI.Invoke(InputUtils.IsDeviceMouse(context));
        }

        public void OnMiddleClick(InputAction.CallbackContext context) {
            if(context.phase != InputActionPhase.Performed) { return; }
            
            MiddleClickUI.Invoke();
        }

        public void OnScrollWheel(InputAction.CallbackContext context) {
            ScrollWheelUI.Invoke(context.ReadValue<Vector2>());
        }
        public void OnRightClick(InputAction.CallbackContext context) {
            if (context.phase != InputActionPhase.Performed) { return; }
            
            RightClickUI.Invoke();
        }

        #endregion
        
        public void EnablePlayerActions() {
            InputActions ??= new InputSystem_Actions();
            
            InputActions.Player.SetCallbacks(this);
            InputActions.UI.SetCallbacks(this);
            
            AddActionMap(ActionMapName.Player, InputActions.Player);
            AddActionMap(ActionMapName.UI, InputActions.UI);
            // TODO: Add more action maps here
            
            switch (initialActionMap) {
                case ActionMapName.Player:
                    InputActions.Player.Enable();
                    _currentActionMap = ActionMapName.Player;
                    break;
                case ActionMapName.UI:
                    InputActions.UI.Enable();
                    _currentActionMap = ActionMapName.UI;
                    break;
                default: 
                    Debug.LogError("ActionMapName is not Added to the switch statement");
                    break;
            }
        }
        
        public void EnableActionMap(ActionMapName mapName) {
            if (_actionMaps.TryGetValue(mapName, out var actionMap)) {
                actionMap.Enable();
            }
        }
        
        public void DisableActionMap(ActionMapName mapName) {
            if (_actionMaps.TryGetValue(mapName, out var actionMap)) {
                actionMap.Disable();
            }
        }
        
        public enum ActionMapName { Player, UI }
        void AddActionMap(ActionMapName mapName, InputActionMap actionMap) {
            if (!_actionMaps.TryAdd(mapName, actionMap)) {
                Debug.LogWarning($"Action map {mapName} already exists. Skipping addition.");
            }
        }

        public void SwitchActionMap(ActionMapName newMap) {
            if (InputActions == null) {
                Debug.LogError("InputActions not initialized");
                return;
            }

            if (_actionMaps.TryGetValue(_currentActionMap, out var currentActionMap)) {
                currentActionMap.Disable();
            }

            if (_actionMaps.TryGetValue(newMap, out var newActionMap)) {
                newActionMap.Enable();
                _currentActionMap = newMap;
            }else {
                Debug.LogError($"Action map {newMap} not found. Make sure to add it to the InitializeActionMaps method.");
            }
        }
        
        /// <returns> a list of the action maps that are disabled.</returns>
        public List<ActionMapName> GetAllInactiveActionMaps() {
            List<ActionMapName> disabledMaps = new List<ActionMapName>();

            foreach (var kvp in _actionMaps) {
                if (kvp.Value.enabled) {
                    disabledMaps.Add(kvp.Key);
                }
            }

            return disabledMaps;
        }

        public bool IsActionMapActive(ActionMapName mapName) {
            return _currentActionMap == mapName;
        } 
        
        public string[] GetActionMapNames() {
            var names = new string[_actionMaps.Count];
            var index = 0;
            foreach (var kvp in _actionMaps) {
                names[index] = kvp.Key.ToString();
                index++;
            }

            return names;
        }
    }
}
