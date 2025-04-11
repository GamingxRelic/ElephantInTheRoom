using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToasterInteraction : MonoBehaviour
{
    bool has_bread = false;
    bool toast_ready = false;
    [SerializeField] float time_to_toast = 1.0f;

    [SerializeField] AudioSource toaster_audio;
    [SerializeField] AudioSource toast_crunch;

    [SerializeField] SpriteRenderer sprite_renderer;
    [SerializeField] Sprite base_sprite;
    [SerializeField] Animator animator;

    public void OnInteract()
    {
        if (!has_bread &&
            PlayerController.instance.held_object != null &&
            PlayerController.instance.held_object.id == "bread")
        {
            TakeBread();
        }
        else if (has_bread && toast_ready)
        {
            GiveToast();
        }
    }

    private void TakeBread()
    {
        if(PlayerController.instance.held_object != null && PlayerController.instance.held_object.id == "bread")
        {
            GameObject obj = PlayerController.instance.held_object.gameObject;
            obj.transform.SetParent(null);
            Destroy(obj);
            PlayerController.instance.held_object = null;
        }

        toaster_audio.Play();
        animator.SetTrigger("Toast");
        has_bread = true;
        StartCoroutine(CookTimer(time_to_toast)); // Toast bread
    }

    private void GiveToast()
    {
        // When the player interacts with the toaster
        // and toast_ready is true, the player will
        // eat the toast.
        toast_crunch.Play();
        animator.SetTrigger("Idle");
        has_bread = false;
        toast_ready = false;
    }

    private IEnumerator CookTimer(float wait_time)
    {
        yield return new WaitForSeconds(wait_time);
        animator.SetTrigger("Finish");
        toast_ready = true;
    }
}
