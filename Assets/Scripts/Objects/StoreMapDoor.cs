using System;
using UnityEngine;

public class StoreMapDoor : MonoBehaviour
{
    Door door;
    private void OnTriggerEnter(Collider other)
    {
        if (door == null)
            door = transform.parent.GetComponent<Door>();
        door.ExitRoomOpen();
    }
}
