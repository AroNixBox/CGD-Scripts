using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime {
    public class UserHubEntryView : MonoBehaviour {
        [SerializeField, Required] TMP_Text usernameTextField;
        [SerializeField, Required] Image userIconImage;

        public void SetUserIcon(Sprite sprite) => userIconImage.sprite = sprite;
        public void SetUsername(string text) => usernameTextField.text = text;
    }
}
