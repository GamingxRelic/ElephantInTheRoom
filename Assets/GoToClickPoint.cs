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
        ai.enableRotation = false;
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

    private void Update()
    {
        if (ai.reachedDestination)
        {
            // Get the trigger capsule collider attached to this GameObject
            CapsuleCollider2D triggerCollider = GetComponent<CapsuleCollider2D>();

            if (triggerCollider != null)
            {
                // Check for objects with the "PickableObject" tag within the trigger collider's bounds
                Collider2D[] hitColliders = Physics2D.OverlapCapsuleAll(
                    triggerCollider.bounds.center,
                    triggerCollider.size,
                    CapsuleDirection2D.Vertical,
                    0f
                );

                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("PickableObject"))
                    {
                        // Destroy the object
                        Destroy(hitCollider.gameObject);
                        break; // Exit after deleting the first object in range
                    }
                }
            }
        }
    }
}