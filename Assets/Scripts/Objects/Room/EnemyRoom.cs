using UnityEngine;

public class EnemyRoom :Room
{
    private EnemySpawner _spawner;
    public int spawnCount;
    public EnemySpawner Spawner
    {
        get
        {
            if (_spawner == null)
                _spawner =GetComponentInChildren<EnemySpawner>();
            return _spawner;
        }
    }
}