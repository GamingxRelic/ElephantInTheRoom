using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Animator animator;

    [SerializeField] float speed = 10f;
    [SerializeField] float acceleration = 0.5f;
    [SerializeField] float deceleration = 0.7f;
    bool flipped = false; // If the player is flipped or not (for animations)

    public static PlayerController instance; // Main instance of this object

    public PickableObject held_object = null;
    public Transform hand_point; // Transform of where held items will be positioned at.
    public Transform drop_point; // Transform of where held items will be dropped from.

    List<PickableObject> pickable_objects_in_range = new List<PickableObject>();
    List<InteractionObject> interaction_objects_in_range = new List<InteractionObject>();

    // Controls
    private PlayerInputActions player_input;
    private InputAction move; 
    private InputAction interact;

    // Interactions
    [SerializeField] private ParticleSystem water_dripping_particles;

    private void Awake()
    {
        player_input = new PlayerInputActions();
    }

    private void OnEnable()
    {
        move = player_input.Player.Move;
        move.Enable();

        interact = player_input.Player.Interact;
        interact.Enable();
        player_input.Player.Interact.performed += InteractPerformed;
    }

    private void OnDisable()
    {
        move.Disable();
        interact.Disable();
    }

    void Start()
    {
        if (!instance)
        {
            instance = this;
        }

        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }


    private void Update()
    {
        if(held_object != null)
        {
            held_object.GetComponent<SpriteRenderer>().sortingOrder = sprite.sortingOrder; //+ 1;
        }
    }

    void FixedUpdate()
    {
        Vector2 movement = move.ReadValue<Vector2>();

        if (movement.magnitude > 1.0f)
        {
            movement.Normalize();
        }

        Vector2 vel = rb.velocity;

        if (movement.magnitude > 0)
        {
            rb.velocity = Vector2.Lerp(vel, movement * speed, acceleration);
        }
        else
        {
            rb.velocity = Vector2.Lerp(vel, movement * speed, deceleration);
        }

        if (rb.velocity.x > 1.0f)
        {
            //sprite.flipX = true;
            if(!flipped)
            {
                flipped = true;
                transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
            }
        }
        else if (rb.velocity.x < -1.0f)
        {
            //sprite.flipX = false;
            if (flipped)
            {
                flipped = false;
                transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
            }
        }

        if (rb.velocity.magnitude > 0.1f)
        {
            animator.SetBool("is_moving", true);
        }
        else
        {
            animator.SetBool("is_moving", false);
        }
    }

    private void InteractWithObject()
    {
        if (interaction_objects_in_range.Count > 0)
        {
            interaction_objects_in_range[0].Interact();
        }
    }


    // Pickable Objects

    private void PickupObject() 
    {
        if (pickable_objects_in_range.Count > 0)
        {
            if (held_object != null)
            {
                DropObject(pickable_objects_in_range[0].transform); // Drop the current held object before picking up a new one.
            }
            // Pick up the first object in range
            held_object = pickable_objects_in_range[0];
            held_object.transform.SetParent(hand_point);
            held_object.transform.position = hand_point.position;


        }
    }


    public void DropObject()
    {
        if (held_object != null)
        {
            held_object.transform.SetParent(null);
            held_object.transform.position = drop_point.position; // Drop the object at the drop point
            held_object.transform.localScale = held_object.transform.localScale.Abs(); // Drop the object at the drop point
            held_object.GetComponent<SpriteRenderer>().sortingOrder = sprite.sortingOrder;
            held_object = null;
            

            // NOTE: This may cause issues later when switching scenes while holding the same object.
            // THis would only be a problem if we keep the same player instance and switch scenes.
            // Take a look at SceneManager.MoveObjectToScene for all
            // children objects of PlayerController.instance.hand_point
        }
    }

    public void DropObject(Transform transf) // For dropping objects at a specific location, usually swapping position with another object.
    {
        if (held_object != null)
        {
            held_object.transform.SetParent(null);
            held_object.transform.position = transf.position; // Drop the object at the specified point
            held_object.GetComponent<SpriteRenderer>().sortingOrder = sprite.sortingOrder;
            held_object = null;


            // NOTE: This may cause issues later when switching scenes while holding the same object.
            // THis would only be a problem if we keep the same player instance and switch scenes.
            // Take a look at SceneManager.MoveObjectToScene for all
            // children objects of PlayerController.instance.hand_point
        }
    }

    private void InteractPerformed(InputAction.CallbackContext context)
    {
        if (pickable_objects_in_range.Count > 0)
        {
            PickupObject();
        }
        else if (interaction_objects_in_range.Count > 0)
        {
            InteractWithObject();
        }
        else if (held_object != null)
        {
            DropObject();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("InteractionObject"))
        {
            InteractionObject interaction_object = collision.gameObject.GetComponent<InteractionObject>();
            interaction_objects_in_range.Add(interaction_object);
        }
        else if(collision.gameObject.CompareTag("PickableObject"))
        {
            PickableObject pickable_object = collision.gameObject.GetComponent<PickableObject>();
            pickable_objects_in_range.Add(pickable_object);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("InteractionObject"))
        {
            InteractionObject interaction_object = collision.gameObject.GetComponent<InteractionObject>();
            interaction_objects_in_range.Remove(interaction_object);
        }
        else if (collision.gameObject.CompareTag("PickableObject"))
        {
            PickableObject pickable_object = collision.gameObject.GetComponent<PickableObject>();
            pickable_objects_in_range.Remove(pickable_object);
        }
    }

    public void StartDrippingWater()
    {
        if (water_dripping_particles != null)
        {
            water_dripping_particles.Play();
        }
    }

    public IEnumerator StartDrippingWater(float duration)
    {
        if (water_dripping_particles != null && !water_dripping_particles.isPlaying)
        {
            water_dripping_particles.Play();
        }
        yield return new WaitForSeconds(duration);
        if (water_dripping_particles != null)
        {
            water_dripping_particles.Stop();
        }
    }

    public void StopDrippingWater()
    {
        if (water_dripping_particles != null)
        {
            water_dripping_particles.Stop();
        }
    }


}
