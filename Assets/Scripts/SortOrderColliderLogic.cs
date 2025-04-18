using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortOrderColliderLogic : MonoBehaviour
{
    [SerializeField] private int enter_y_order = 0; // The y order to set when the player enters the collider
    [SerializeField] private int exit_y_order = 0; // The y order to set when the player enters the collider

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<YSortController>().SetYOrder(enter_y_order);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<YSortController>().SetYOrder(exit_y_order);
        }
    }
}
