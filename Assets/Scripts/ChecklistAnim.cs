using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChecklistAnim : MonoBehaviour
{
    // Controls
    private PlayerInputActions player_input;
    private InputAction toggle_checklist;

    // Animator
    private Animator anim;
    private bool is_open = false;

    // Time to wait when auto pull out checklist
    [SerializeField] private float show_hide_checklist_time = 1.5f;

    // Audio
    [SerializeField] private AudioSource open_sound;
    [SerializeField] private AudioSource close_sound;

    private void Awake()
    {
        player_input = new PlayerInputActions();
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        toggle_checklist = player_input.Player.ToggleChecklist;
        toggle_checklist.Enable();
        toggle_checklist.performed += OnToggleChecklist;

    }


    private void OnDisable()
    {
        toggle_checklist.Disable();
        toggle_checklist.performed -= OnToggleChecklist;
    }

    private void OnToggleChecklist(InputAction.CallbackContext context)
    {
        if(PauseMenuLogic.is_paused)
            return; // If game paused return

        if (!is_open)
        {
            open_sound.Play();
            anim.SetTrigger("Open");
            is_open = true;
        }
        else
        {
            close_sound.Play();
            anim.SetTrigger("Close");
            is_open = false;
        }
    }

    public void OnToggleChecklist()
    {
        if (PauseMenuLogic.is_paused)
            return; // If game paused return

        if (!is_open)
        {
            open_sound.Play();
            anim.SetTrigger("Open");
            is_open = true;
        }
        else
        {
            close_sound.Play();
            anim.SetTrigger("Close");
            is_open = false;
        }
    } // Same method just with no parameters 

    public IEnumerator ShowThenHide()
    {
        if (is_open)
            yield break;

        anim.SetTrigger("Open");
        is_open = true;

        yield return new WaitForSeconds(show_hide_checklist_time);

        if (!is_open)
            yield break;

        anim.SetTrigger("Close");
        is_open = false;
    }

}
