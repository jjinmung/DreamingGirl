using UnityEngine;

public class EventRoom : Room
{
    [SerializeField] private ChestSpawner chestSpawner;
    public void EventInit()
    {
        chestSpawner.Init();
    }
}
