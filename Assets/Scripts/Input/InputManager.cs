using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerControls Controls;
    // Start is called before the first frame update
    void Awake() {
        if (Controls == null) {
            Controls = new PlayerControls();
            Controls.Enable();
        }
    }
}
