using UnityEngine;

public class EventRoom : Room
{
    public void ClearEvent()
    {
        Managers.Stage.ClearRoom();
    }
}
