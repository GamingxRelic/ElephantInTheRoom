using UnityEngine;
// Use the Pathfinding namespace to be able to
// use most common classes in the package
using Pathfinding;
using UnityEngine.InputSystem;

public class FollowCursor : MonoBehaviour
{
    AILerp ai;

    // Controls
    private PlayerInputActions player_input;
    private InputAction fire;


    private void Awake()
    {
        player_input = new PlayerInputActions();
    }

    // This runs when the game starts
    void OnEnable()
    {
        // Get a reference to our movement script.
        // You can alternatively use the interface IAstarAI,
        // which makes the code work with all movement scripts
        ai = GetComponent<AILerp>();
        fire = player_input.Player.Fire;
        fire.Enable();
        player_input.Player.Fire.performed += InteractPerformed;
    }

    private void OnDisable()
    {
        fire.Disable();
    }

    private void InteractPerformed(InputAction.CallbackContext context)
    {
        // Get the mouse position
        var mousePosition = Input.mousePosition;

        Vector3 point = Camera.main.ScreenToWorldPoint(mousePosition);

        ai.destination = point;
    }
}