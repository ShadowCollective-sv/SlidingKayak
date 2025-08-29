using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Runtime.Scene;

public enum GameState
{
    Menu,
    Playing,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState currentState;

    [SerializeField] private LevelLoadManager LevelLoadPrefab; //указывает на ресурс
    [HideInInspector] public LevelLoadManager _levelLoadManager; //указывает на инстанс

    [SerializeField] private AnalyticsManager _analyticsManager;


[Header("Названия сцен")]
    public string menuSceneName = "MainMenu";
    public string level1SceneName = "Level_01";

    [Header("Экраны UI")]
    public GameObject pauseMenuUI;
    public GameObject loadingScreenUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        _levelLoadManager = Instantiate(LevelLoadPrefab); //если бы тут было LevelLoadPrefab = Instantiate(LevelLoadPrefab), то мы потеряли бы ссылку на оригинальный префаб в рантайме.
        _levelLoadManager.LoadScene(menuSceneName);

        _analyticsManager = Instantiate(_analyticsManager); //прям перезаписываем в вызов, мне путь на префаб теперь не нужен
    }

    // <-- 2. Методы для кнопок теперь 'async void'
    // Это специальный тип для событий, вроде нажатия на кнопку.
    public async void StartGame()
    {
        // Вместо StartCoroutine, теперь мы просто вызываем наш async-метод.
        await LoadSceneAndSwitchState(level1SceneName, GameState.Playing);
        
        _levelLoadManager.LoadScene(level1SceneName);
        
    }

    public async void GoToMainMenu()
    {
        Time.timeScale = 1f;
        await LoadSceneAndSwitchState(menuSceneName, GameState.Menu);
    }

    // Метод паузы не асинхронный, он остаётся без изменений.
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            pauseMenuUI.SetActive(true);
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            pauseMenuUI.SetActive(false);
        }
    }

    // <-- 3. Главное изменение здесь!
    // Вместо 'IEnumerator' теперь 'async UniTask'.
    private async UniTask LoadSceneAndSwitchState(string sceneName, GameState newState)
    {
        if (loadingScreenUI != null)
            loadingScreenUI.SetActive(true);

        // Вместо цикла 'while' теперь просто одна строчка с 'await'!
        // Она "ждёт", пока сцена не загрузится, и только потом идёт дальше.
        await SceneManager.LoadSceneAsync(sceneName);

        // Этот код выполнится только ПОСЛЕ завершения загрузки сцены.
        if (loadingScreenUI != null)
            loadingScreenUI.SetActive(false);

        currentState = newState;

        if (newState == GameState.Playing)
        {
            pauseMenuUI = GameObject.FindGameObjectWithTag("PauseMenu");
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
}