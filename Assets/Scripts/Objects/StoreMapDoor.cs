using System;
using UnityEngine;

public class StoreMapDoor : MonoBehaviour
{
    private bool isOpening;
    Door door;

    private void OnEnable()
    {
        isOpening= false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isOpening) return;
        if (door == null)
            door = transform.parent.GetComponent<Door>();
        door.ExitRoomOpen();
        isOpening = true;
    }
}
