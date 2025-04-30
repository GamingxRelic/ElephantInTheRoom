using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableObject : MonoBehaviour
{
    public string id = ""; // What is this object's ID? Ex. toast, rock, string, etc.

    [SerializeField] private LayerMask interactableLayers;

    private bool outlined = false;

    [SerializeField] private Material outline_material;
    [SerializeField] private Material default_material;

    public bool Outlined
    {
        get => outlined;
        set
        {
            if (value)
            {
                GetComponent<SpriteRenderer>().material = outline_material;
            }
            else
            {
                GetComponent<SpriteRenderer>().material = default_material;
            }

            outlined = value;
        }
    }

}
