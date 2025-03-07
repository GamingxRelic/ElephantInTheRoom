using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkInteraction : MonoBehaviour
{
    Animator anim; // Has animations for ON, OFF
    bool on = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnInteract()
    {
        if (on &&
            PlayerController.instance.held_object != null &&
            PlayerController.instance.held_object.id == "burnt_toast")
        {
            TakeToast();
        }
        else if (on)
            TurnOff();
        else
            TurnOn();

    }

    void TakeToast()
    {
        if (PlayerController.instance.held_object != null && PlayerController.instance.held_object.id == "burnt_toast")
        {
            GameObject obj = PlayerController.instance.held_object.gameObject;
            obj.transform.SetParent(null);

            // Get the ParticleSystem child and set its parent transform to null
            ParticleSystem particles = obj.GetComponentInChildren<ParticleSystem>();
            if (particles != null)
            {
                particles.transform.SetParent(null);
                particles.Stop();
                Destroy(particles.gameObject, 1.0f);
            }

            Destroy(obj);
            PlayerController.instance.held_object = null;
        }

        print("Sink took burnt toast!");
    }
    void TurnOff()
    {
        anim.SetTrigger("Off");
        on = false;
    }

    void TurnOn()
    {
        anim.SetTrigger("On");
        on = true;
    }
}
