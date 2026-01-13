using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance; // 싱글톤 인스턴스
    public static Managers Instance { get { Init(); return _instance; } }

    /*
    private ResourceManager _resource = new ResourceManager();
    private SoundManager _sound = new SoundManager();
    private SceneManagerEx _scene = new SceneManagerEx();

    public static ResourceManager Resource => Instance._resource;
    public static SoundManager Sound => Instance._sound;
    public static SceneManagerEx Scene => Instance._scene;*/

    void Start()
    {
        Init();
    }

    static void Init()
    {
        if (_instance == null)
        {
            // 게임 오브젝트가 있는지 먼저 확인
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
            

        }
    }
}