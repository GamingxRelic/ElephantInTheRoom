using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkInteraction : MonoBehaviour
{
    Animator anim; // Has animations for ON, OFF
    bool on = false;

    [SerializeField] private AudioSource water_flow_audio;
    [SerializeField] private AudioSource faucet_squeek_audio;
    [SerializeField] private AudioSource toothbrush_audio;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnInteract()
    {
        if (on)
        {
            if (PlayerController.instance.held_object != null)
            {
                if (PlayerController.instance.held_object.id == "burnt_toast")
                {
                    TakeToast();
                }
                else if (PlayerController.instance.held_object.id == "toothbrush")
                {
                    toothbrush_audio.Play();
                }
            }
            else
            {
                TurnOff();
            }
        }
        else
        {
            TurnOn();
        }
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
        faucet_squeek_audio.Play();
        water_flow_audio.Stop();
        on = false;
    }

    void TurnOn()
    {
        anim.SetTrigger("On");
        faucet_squeek_audio.Play();
        water_flow_audio.Play();
        on = true;
    }
}
