using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public PlayerTetherManager tetherManager;

    public CameraController cameraController;

    private void Start()
    {
        if (Gamepad.all.Count >= 2)
        {
            var player1 = PlayerInput.Instantiate(player1Prefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[0]);
            var player2 = PlayerInput.Instantiate(player2Prefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[1]);

            // Add to camera tracking
            cameraController.AddTarget(player1.transform);
            cameraController.AddTarget(player2.transform);

            // Assign to tether manager
            tetherManager.player1 = player1.transform;
            tetherManager.player2 = player2.transform;
        }
        else
        {
            Debug.LogWarning("Not enough gamepads connected!");
        }
    }
}
