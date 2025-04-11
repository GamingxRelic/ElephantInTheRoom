using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableObject : MonoBehaviour
{
    public string id = ""; // What is this object's ID? Ex. toast, rock, string, etc.

    [SerializeField] private LayerMask interactableLayers;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos2D, Vector2.zero, Mathf.Infinity, interactableLayers);

            print(hit.collider.gameObject.name);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log("clicked " + id);
            }
        }
    }


}
