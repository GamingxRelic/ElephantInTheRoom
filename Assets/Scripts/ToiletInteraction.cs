using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletInteraction : MonoBehaviour
{
    [SerializeField] private AudioSource flush_sound;

    public void OnInteract()
    {
        // Play the flush sound
        if (flush_sound != null)
        {
            flush_sound.Play();

            ChecklistHandler.instance.TriggerGoal("flush_toilet");
        }
    }
}
