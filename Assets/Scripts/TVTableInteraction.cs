using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVTableInteraction : MonoBehaviour
{
    bool on = false;

    [SerializeField] SpriteRenderer tv_on;
    [SerializeField] SpriteRenderer tv_off;

    [SerializeField] AudioSource tv_audio;

    private void Start()
    {
        tv_off.enabled = true;
        tv_on.enabled = false;
    }

    public void OnInteract()
    {
        if (on)
        {
            tv_audio.Stop();
            tv_on.enabled = false;
            tv_off.enabled = true;
        }
        else
        {
            tv_audio.Play();
            tv_on.enabled = true;
            tv_off.enabled = false;

            ChecklistHandler.instance.TriggerGoal("watch_tv");
        }
        on = !on;
    }
}
