using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        if (Gamepad.all.Count >= 2)
        {
            PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[0]);
            PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[1]);
        }
        else
        {
            Debug.LogWarning("Not enough gamepads connected!");
        }
    }
}
