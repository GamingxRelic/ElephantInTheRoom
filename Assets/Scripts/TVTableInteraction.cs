using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TVTableInteraction : MonoBehaviour
{
    bool on = false;

    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Sprite tv_on;
    [SerializeField] Sprite tv_off;

    [SerializeField] AudioSource tv_audio;

    private void Start()
    {
        sprite.sprite = tv_off;
    }

    public void OnInteract()
    {
        if (on)
        {
            tv_audio.Stop();
            sprite.sprite = tv_off;
        }
        else
        {
            tv_audio.Play();
            sprite.sprite = tv_on;

            ChecklistHandler.instance.TriggerGoal("watch_tv");
        }
        on = !on;
    }
}
