using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance; // 싱글톤 인스턴스
    public static Managers Instance { get { Init(); return _instance; } }

    private ObjectPooler _pooler;
    private CameraManager _camera;
    private InputManager _input;
    private PlayerManager _player;
    private UIManager _ui;
    private ResourceManager _resource;
    private SoundManager _sound;
    public static ObjectPooler  Pool => Instance._pooler;
    public static CameraManager Camera => Instance._camera;
    public static InputManager Input => Instance._input;
    public static PlayerManager Player => Instance._player;
    public static UIManager UI => Instance._ui;
    public static ResourceManager Resource => Instance._resource;
    public static SoundManager Sound => Instance._sound;

    void Awake()
    {
        Init();
    }

    static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();

            _instance._pooler = go.GetComponentInChildren<ObjectPooler>();
            _instance._camera = go.GetComponentInChildren<CameraManager>();
            _instance._input = go.GetComponentInChildren<InputManager>();
            _instance._player = go.GetComponentInChildren<PlayerManager>();
            _instance._ui = go.GetComponentInChildren<UIManager>();
            _instance._resource=new ResourceManager();
            _instance._sound = new  SoundManager();
            
            _instance._camera.Init();
        }

    }
}