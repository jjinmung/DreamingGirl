using UnityEngine;

public class ChestSpawner:MonoBehaviour
{
    ChestBase[] ChestPrefabs;

    public void Init()
    {
        if (ChestPrefabs ==null)
        {
            ChestPrefabs = GetComponentsInChildren<ChestBase>();
        }
        foreach (var prefab in ChestPrefabs)
        {
            prefab.gameObject.SetActive(false);
        }

        var index = Random.Range(0, ChestPrefabs.Length);
        ChestPrefabs[index].gameObject.SetActive(true);
        ChestPrefabs[index].Init();
    }
}