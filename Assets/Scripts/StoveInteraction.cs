using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveInteraction : MonoBehaviour
{
    bool has_bread = false;
    bool toast_ready = false;
    [SerializeField] float time_to_toast = 2.0f;
    [SerializeField] GameObject burnt_toast_prefab;

    public void OnInteract()
    {
        if (!has_bread &&
            PlayerController.instance.held_object != null &&
            PlayerController.instance.held_object.id == "bread")
        {
            TakeBread();
        }
        else if (has_bread && toast_ready)
        {
            GiveToast();
        }
    }

    private void TakeBread()
    {
        if (PlayerController.instance.held_object != null && PlayerController.instance.held_object.id == "bread")
        {
            GameObject obj = PlayerController.instance.held_object.gameObject;
            obj.transform.SetParent(null);
            Destroy(obj);
            PlayerController.instance.held_object = null;

            print("Stove took bread!");
            has_bread = true;
            StartCoroutine(CookTimer(time_to_toast)); // Toast bread
        }
    }

    private void GiveToast()
    {
        // When the player interacts with the stove
        // and toast_ready is true, the player will
        // take the burnt toast object (which should
        // be visible) and now must put it in the
        // sink.

        if (PlayerController.instance.held_object != null)
        {
            PlayerController.instance.DropObject();
        }

        GameObject obj = Instantiate(burnt_toast_prefab, PlayerController.instance.hand_point.position, Quaternion.identity);
        PlayerController.instance.held_object = obj.GetComponent<PickableObject>();
        obj.transform.SetParent(PlayerController.instance.hand_point);

        has_bread = false;
        toast_ready = false;
    }

    private IEnumerator CookTimer(float wait_time)
    {
        yield return new WaitForSeconds(wait_time);
        toast_ready = true;
    }
}
