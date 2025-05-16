using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionObject : MonoBehaviour
{
    [SerializeField] private UnityEvent onInteract;

    private bool outlined = false;

    [SerializeField] private Material outline_material;
    [SerializeField] private Material red_outline_material;
    [SerializeField] private Material default_material;

    public bool use_red_outline = false;

    public bool Outlined
    {
        get => outlined;
        set
        {
            if (value)
            {
                if (use_red_outline)
                    GetComponent<SpriteRenderer>().material = red_outline_material;
                else
                    GetComponent<SpriteRenderer>().material = outline_material;
            }
            else
            {
                GetComponent<SpriteRenderer>().material = default_material;
            }

            outlined = value;
        }
    }

    public void Interact()
    {
        onInteract?.Invoke();
    }

}
