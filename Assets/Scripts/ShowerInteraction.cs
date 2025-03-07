using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowerInteraction : MonoBehaviour
{
    [SerializeField] private ParticleSystem shower_particles;
    bool on = false; // Is the shower on?
    bool player_in_range = false;

    float wet_player_duration = 3.0f; // How long the player will be wet after leaving the shower

    public void OnInteract()
    {
        if (on)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }

    void TurnOff()
    {
        if ( shower_particles != null)
        {
            shower_particles.Stop();

            if(player_in_range &&
                PlayerController.instance != null)
            {
                StartCoroutine(PlayerController.instance.StartDrippingWater(wet_player_duration));
            }
        }
        on = false;
    }

    void TurnOn()
    {
        if (shower_particles != null)
        {
            shower_particles.Play();
        }
        on = true;

        if (player_in_range &&
            PlayerController.instance != null)
        {
            PlayerController.instance.StartDrippingWater(); // Start dripping water when entering the shower
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        player_in_range = true;

        if(on)
        {
            if (PlayerController.instance != null)
            {
                PlayerController.instance.StartDrippingWater(); // Start dripping water when entering the shower

                PickableObject held_object = PlayerController.instance.held_object;
                if (held_object != null &&
                    held_object.id == "burnt_toast")
                {
                    ParticleSystem burning_particles = held_object.gameObject.GetComponentInChildren<ParticleSystem>();
                    burning_particles.Stop();
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        player_in_range = false;
        if(on && PlayerController.instance != null)
        {
            StartCoroutine(PlayerController.instance.StartDrippingWater(wet_player_duration));
        }
    }

}
