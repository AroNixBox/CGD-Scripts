using UnityEngine.InputSystem;

namespace Aroboss.Core.Runtime.Service.Input {
    public static class InputUtils{
        // Helper method to check if the input device is a mouse
        public static bool IsDeviceMouse(InputAction.CallbackContext context) {
            
            // Debug.Log($"Device name: {context.control.device.name}");
            return context.control.device.name == "Mouse";
        }
    }
}
