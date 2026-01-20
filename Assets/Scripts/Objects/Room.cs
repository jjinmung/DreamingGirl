using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<Door> doors;
    public Door EnterDoor;
    public Transform SpawnPos =>EnterDoor.EnterPos;
    
}
