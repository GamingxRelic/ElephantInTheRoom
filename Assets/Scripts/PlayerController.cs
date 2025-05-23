﻿using Cinemachine.Utility;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

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

    // Audio
    [SerializeField] private AudioSource pickup_item_sound;
    [SerializeField] private AudioSource drop_item_sound;

    // Controls
    private PlayerInputActions player_input;
    private InputAction move; 
    private InputAction interact;
    private InputAction drop;

    // Interactions
    [SerializeField] private ParticleSystem water_dripping_particles;
    private InteractionObject queued_interaction = null;
    private PickableObject queued_pickup = null;



    // Point and click controls
    private AILerp ai;
    private Seeker seeker;

    private InputAction click_to_move;
    private bool using_pathfinding = false;
    private enum ControlMode { Manual, Pathfinding }
    private ControlMode control_mode = ControlMode.Manual;
    [SerializeField] private LayerMask clickable_layers;
    private MonoBehaviour hovered_object = null;



    private void Awake()
    {
        player_input = new PlayerInputActions();
        ai = GetComponent<AILerp>();
        seeker = GetComponent<Seeker>();
    }

    private void OnEnable()
    {
        move = player_input.Player.Move;
        move.Enable();

        drop = player_input.Player.Drop;
        drop.Enable();
        drop.performed += (context) => DropObject();

        click_to_move = player_input.Player.Fire;
        click_to_move.Enable();
        click_to_move.performed += OnClickToMove;

        interact = player_input.Player.Interact;
        interact.Enable();
        player_input.Player.Interact.performed += InteractPerformed;

    }


    private void OnDisable()
    {
        move.Disable();
        interact.Disable();

        click_to_move.Disable();
        click_to_move.performed -= OnClickToMove;
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

        ai.speed = speed;
    }


    private void Update()
    {
        if(PauseMenuLogic.is_paused)
            return;

        if (held_object != null)
        {
            held_object.GetComponent<SpriteRenderer>().sortingOrder = sprite.sortingOrder; //+ 1;
        }

        if (control_mode == ControlMode.Pathfinding)
        {
            animator.SetBool("is_moving", ai.velocity.magnitude > 0.1f);
        }
        else
        {
            animator.SetBool("is_moving", rb.velocity.magnitude > 0.1f);
        }

        // Hovered objects outline logic
        Vector3 mouse_world_pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouse_world_pos.z = 0;

        Collider2D hit = Physics2D.OverlapPoint(mouse_world_pos, clickable_layers);

        MonoBehaviour new_hover = null;

        if (hit != null)
        {
            new_hover = hit.GetComponentInParent<InteractionObject>();
            if (new_hover == null)
                new_hover = hit.GetComponentInParent<PickableObject>();
        }

        // --- REMOVE outline from previously hovered object ONLY if not in trigger list ---
        if (hovered_object != null && hovered_object != new_hover)
        {
            bool stillInRange = false;

            if (hovered_object is InteractionObject oldInt && interaction_objects_in_range.Contains(oldInt))
                stillInRange = true;
            else if (hovered_object is PickableObject oldPick && pickable_objects_in_range.Contains(oldPick))
                stillInRange = true;

            if (!stillInRange)
            {
                if (hovered_object is InteractionObject oldIntObj)
                    oldIntObj.Outlined = false;
                else if (hovered_object is PickableObject oldPickObj)
                    oldPickObj.Outlined = false;

                hovered_object = null;
            }
        }

        // --- Apply new hovered outline ---
        if (new_hover != null && new_hover != hovered_object)
        {
            if (new_hover is InteractionObject newIntObj)
                newIntObj.Outlined = true;
            else if (new_hover is PickableObject newPickObj)
                newPickObj.Outlined = true;

            hovered_object = new_hover;
        }



    }

    void FixedUpdate()
    {
        if (PauseMenuLogic.is_paused)
            return; // If game paused return

        if (using_pathfinding)
        {
            // Stop applying Rigidbody movement when using pathfinding
            rb.velocity = Vector2.zero;

            // Check if we've arrived
            if (ai.reachedEndOfPath || Vector2.Distance(transform.position, ai.destination) < 0.1f)
            {
                using_pathfinding = false;
            }
            return;
        }

        Vector2 movement = move.ReadValue<Vector2>();

        // If the player manually moves, switch to manual mode
        if (movement.magnitude > 0.1f && control_mode != ControlMode.Manual)
        {
            control_mode = ControlMode.Manual;
            ai.destination = transform.position; // Stop AILerp
            ai.canMove = false;

            // Clear queued interaction object.
            queued_interaction = null;
        }

        if (control_mode == ControlMode.Manual)
        {
            Vector2 vel = rb.velocity;

            if (movement.magnitude > 1.0f)
                movement.Normalize();

            if (movement.magnitude > 0)
                rb.velocity = Vector2.Lerp(vel, movement * speed, acceleration);
            else
                rb.velocity = Vector2.Lerp(vel, movement * speed, deceleration);
        }
        else if (control_mode == ControlMode.Pathfinding)
        {
            rb.velocity = Vector2.zero;

            // Flip based on AILerp velocity
            if (ai.velocity.x > 0.1f && !flipped)
            {
                flipped = true;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -1f, transform.localScale.y, transform.localScale.z);
            }
            else if (ai.velocity.x < -0.1f && flipped)
            {
                flipped = false;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            if (queued_interaction == null &&
                (ai.reachedEndOfPath || Vector2.Distance(transform.position, ai.destination) < 0.1f))
            {
                control_mode = ControlMode.Manual;
                ai.canMove = false;
            }
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


    private void OnClickToMove(InputAction.CallbackContext context)
    {
        StartCoroutine(HandleClickToMove());
    }

    private IEnumerator HandleClickToMove() // So that the IsPointerOverGameObject() can work properly, I moved this to a coroutine that will wait for a frame before doing all the logic.
    {
        yield return null;

        // If game is paused, return
        if (PauseMenuLogic.is_paused)
            yield break;

        // Prevent clickthrough of UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            yield break;

        // Get clicked object
        Vector3 click_screen_pos = Mouse.current.position.ReadValue();
        Vector3 click_world_pos = Camera.main.ScreenToWorldPoint(click_screen_pos);
        click_world_pos.z = 0;

        Collider2D hit = Physics2D.OverlapCircle(click_world_pos, 0.1f, clickable_layers);

        // Ignore self-click
        if (hit != null && hit.gameObject == gameObject)
            yield break;

        if (hit != null)
        {
            // InteractionObject
            var target_interaction = hit.GetComponentInParent<InteractionObject>();
            if (target_interaction != null)
            {
                if (interaction_objects_in_range.Contains(target_interaction))
                {
                    target_interaction.Interact();
                    yield break;
                }

                // Not in range — pathfind
                queued_interaction = target_interaction;
                queued_pickup = null;
                StartPathTo(target_interaction.transform.position);
                yield break;
            }

            // PickableObject
            var target_pickable = hit.GetComponentInParent<PickableObject>();
            if (target_pickable != null)
            {
                if (pickable_objects_in_range.Contains(target_pickable))
                {
                    if (held_object != null)
                        DropObject(target_pickable.transform);

                    held_object = target_pickable;
                    held_object.transform.SetParent(hand_point);
                    held_object.transform.position = hand_point.position;
                    pickup_item_sound.Play();

                    // Quick and dirty way to check if music discs are being picked up and if so, complete the goal.
                    string[] music_discs = { "pink_music_disc", "blue_music_disc", "green_music_disc", "yellow_music_disc", "red_music_disc", "purple_music_disc" };
                    foreach (string disc in music_discs)
                    {
                        if (held_object.id == disc)
                        {
                            ChecklistHandler.instance.TriggerGoal(disc);
                            break;
                        }
                    }

                    yield break;
                }

                // Not in range — pathfind
                queued_pickup = target_pickable;
                queued_interaction = null;
                StartPathTo(target_pickable.transform.position);
                yield break;
            }
        }

        // No interactables or pickables — just move to point
        queued_interaction = null;
        queued_pickup = null;
        StartPathTo(click_world_pos);
    }


    // This is a helper function I made for when we want to move to a specific point using point and click controls.
    private void StartPathTo(Vector3 targetPos)
    {
        ai.Teleport(transform.position);
        ai.destination = targetPos;
        ai.SearchPath();
        ai.canMove = true;
        control_mode = ControlMode.Pathfinding;
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
            pickup_item_sound.Play();


            // Quick and dirty way to check if music discs are being picked up and if so, complete the goal.
            string[] music_discs = { "pink_music_disc", "blue_music_disc", "green_music_disc", "yellow_music_disc", "red_music_disc", "purple_music_disc"};
            foreach (string disc in music_discs)
            {
                if (held_object.id == disc)
                {
                    ChecklistHandler.instance.TriggerGoal(disc);
                    break;
                }
            }
        }
    }


    public void DropObject()
    {
        if (PauseMenuLogic.is_paused)
            return; // If game paused return

        if (held_object != null)
        {
            held_object.transform.SetParent(null);
            held_object.transform.position = drop_point.position; // Drop the object at the drop point
            held_object.transform.localScale = held_object.transform.localScale.Abs(); // Drop the object at the drop point
            held_object.GetComponent<SpriteRenderer>().sortingOrder = sprite.sortingOrder;
            held_object = null;
            drop_item_sound.Play();
        }
    }

    public void DropObject(Transform transf) // For dropping objects at a specific location, usually swapping position with another object.
    {
        if (held_object != null)
        {
            held_object.transform.SetParent(null);
            held_object.transform.localScale = held_object.transform.localScale.Abs(); // Drop the object at the drop point
            held_object.transform.position = transf.position; // Drop the object at the specified point
            held_object.GetComponent<SpriteRenderer>().sortingOrder = sprite.sortingOrder;
            held_object = null;
            drop_item_sound.Play();
        }
    }

    private void InteractPerformed(InputAction.CallbackContext context)
    {
        if (PauseMenuLogic.is_paused)
            return; // If game paused return

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
        if (collision.CompareTag("InteractionObject"))
        {
            var obj = collision.GetComponent<InteractionObject>();
            if (!interaction_objects_in_range.Contains(obj))
            {
                interaction_objects_in_range.Add(obj);
            }

            // Refresh outlines
            foreach (var o in interaction_objects_in_range)
            {
                o.Outlined = false;
            }

            if (interaction_objects_in_range.Count > 0)
            {
                interaction_objects_in_range[0].Outlined = true;
            }

            // If we were trying to reach this one, interact now
            if (obj == queued_interaction)
            {
                obj.Interact();
                queued_interaction = null;
                ai.canMove = false;
                control_mode = ControlMode.Manual;
            }
        }
        else if (collision.CompareTag("PickableObject"))
        {
            var obj = collision.GetComponent<PickableObject>();
            if (!pickable_objects_in_range.Contains(obj))
            {
                pickable_objects_in_range.Add(obj);
            }

            foreach (var o in pickable_objects_in_range)
            {
                o.Outlined = false;
            }

            if (pickable_objects_in_range.Count > 0)
            {
                pickable_objects_in_range[0].Outlined = true;
            }

            // If we were trying to pick this up, do so now
            if (obj == queued_pickup)
            {
                if (held_object != null)
                    DropObject(obj.transform);

                held_object = obj;
                held_object.transform.SetParent(hand_point);
                held_object.transform.position = hand_point.position;
                pickup_item_sound.Play();


                // Quick and dirty way to check if music discs are being picked up and if so, complete the goal.
                string[] music_discs = { "pink_music_disc", "blue_music_disc", "green_music_disc", "yellow_music_disc", "red_music_disc", "purple_music_disc" };
                foreach (string disc in music_discs)
                {
                    if (held_object.id == disc)
                    {
                        ChecklistHandler.instance.TriggerGoal(disc);
                        break;
                    }
                }

                queued_pickup = null;
                ai.canMove = false;
                control_mode = ControlMode.Manual;
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("InteractionObject"))
        {
            InteractionObject interaction_object = collision.gameObject.GetComponent<InteractionObject>();

            // Always remove and clear outline from the one leaving
            interaction_objects_in_range.Remove(interaction_object);
            interaction_object.Outlined = false;

            // Reassign outline to new [0], if any left
            if (interaction_objects_in_range.Count > 0)
            {
                interaction_objects_in_range[0].Outlined = true;
            }
        }
        else if (collision.gameObject.CompareTag("PickableObject"))
        {
            PickableObject pickable_object = collision.gameObject.GetComponent<PickableObject>();

            pickable_objects_in_range.Remove(pickable_object);
            pickable_object.Outlined = false;

            if (pickable_objects_in_range.Count > 0)
            {
                pickable_objects_in_range[0].Outlined = true;
            }
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
