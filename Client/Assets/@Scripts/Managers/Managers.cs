using UnityEngine;

public class Managers : MonoBehaviour
{
	public static bool Initialized { get; set; }

	private static Managers s_instance; // 유일성이 보장된다
    public static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    #region Contents

    private EventManager _event = new EventManager();
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();
    private MapManager _map = new MapManager();
    private PlayerManager _player = new PlayerManager();

    public static EventManager Event { get { return Instance?._event; } }
    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static MapManager Map { get { return Instance?._map; } }
    public static PlayerManager Player { get { return Instance?._player; } }
    
    #endregion

    #region Core

    private DataManager _data = new DataManager();
    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();
    private NetworkManager _network = new NetworkManager();
    private SaveManager _save = new SaveManager();

    public static DataManager Data { get { return Instance?._data; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static SoundManager Sound { get { return Instance?._sound; } }
    public static UIManager UI { get { return Instance?._ui; } }
    public static NetworkManager Network { get { return Instance?._network; } }
    public static SaveManager Save { get { return Instance?._save; } }

    #endregion

    #region Language
    private static Define.ELanguage _language = Define.ELanguage.Korean;
    public static Define.ELanguage Language
    {
        get { return _language; }
        set
        {
            _language = value;
        }
    }

    public static string GetText(string textId)
    {
        switch (_language)
        {
            case Define.ELanguage.Korean:
                return Data.TextDict[textId].KOR;
            case Define.ELanguage.English:
                break;
            case Define.ELanguage.French:
                break;
            case Define.ELanguage.SimplifiedChinese:
                break;
            case Define.ELanguage.TraditionalChinese:
                break;
            case Define.ELanguage.Japanese:
                break;
        }

        return "";
    }
    #endregion

    public static void Init()
    {
        if (s_instance == null && Initialized == false)
        {
            Initialized = true;

			GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();
            s_instance._sound.Init();
            s_instance._player.Init();
            s_instance._save.Init();
        }		
	}

	public void Update()
	{
		_network?.Update();
        _game?.Update();
    }

	public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
    }
}
