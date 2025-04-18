using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YSortController : MonoBehaviour
{
    SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }
    public void SetYOrder(int order)
    {
        sprite.sortingOrder = order;
    }

}
