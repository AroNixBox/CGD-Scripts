using UnityEngine;

//Preset für UI Menüswitches
[System.Serializable]
public class MenuSwitchButton : MonoBehaviour {
    [SerializeField] private GameObject currentMenu;
    [SerializeField] private GameObject otherMenu;

    public GameObject OtherMenu { get => otherMenu; set => otherMenu = value; }
    public GameObject CurrentMenu { get => currentMenu; set => currentMenu = value; }
}
