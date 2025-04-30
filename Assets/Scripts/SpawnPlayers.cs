using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public PlayerTetherManager tetherManager;

    public CameraController cameraController;

    private void Start()
    {
        if (Gamepad.all.Count >= 2)
        {
            var player1 = PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[0]);
            // Make player1 red
            Renderer p1Renderer = player1.GetComponentInChildren<Renderer>();
            if (p1Renderer != null)
            {
                p1Renderer.material.color = Color.red;
            }
            var player2 = PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[1]);
            // Make player2 blue
            Renderer p2Renderer = player2.GetComponentInChildren<Renderer>();
            if (p2Renderer != null)
            {
                p2Renderer.material.color = Color.blue;
            }

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
