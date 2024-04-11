using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;
    static Managers Instance { get { Init(); return _instance; } }

    #region Contents
    ObjectManager _obj = new ObjectManager();
    NetworkManager _network = new NetworkManager();
    SettingManager _setting = new SettingManager();
    UserManager _user = new UserManager();
    RoomManager _room = new RoomManager();
    UIManager _ui = new UIManager();
    GameManager _game = new GameManager();

    public static ObjectManager Object { get { return Instance._obj; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static SettingManager Setting { get { return Instance._setting; } }
    public static UserManager User { get { return Instance._user; } }
    public static RoomManager Room { get { return Instance._room; } }
    public static UIManager UI { get { return Instance._ui; } }
    public static GameManager Game { get { return Instance._game; } }
    #endregion

    #region Core
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();

    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
        _network.Update();
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

            _instance._pool.Init();
            _instance._network.Init();
            _instance._user.Init();
        }
    }

    public static void Clear()
    {
        Scene.Clear();
        Pool.Clear();
    }
}
