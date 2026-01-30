using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Door> doors;
    public Door EnterDoor;
    public Transform SpawnPos =>EnterDoor.EnterPos;

    public void CloseImmediately()
    {
        //풀링을 위해 다시 닫아놓는다.
        foreach (var door in doors)
        {
            door.CloseImmediately();
        }
    }
}
