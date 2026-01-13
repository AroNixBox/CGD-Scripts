using UnityEngine;

namespace UI.Runtime {
    public class MenuSwitchSimple : MonoBehaviour {
        private void UIClickSound() {
            //audioManager.PlaySfx("UI Click", 3, null, 0);
        }
        public void SwitchScreen(MenuSwitchButton info) {
            UIClickSound();
            info.CurrentMenu.SetActive(false);
            info.OtherMenu.SetActive(true);
        }
    }
}
