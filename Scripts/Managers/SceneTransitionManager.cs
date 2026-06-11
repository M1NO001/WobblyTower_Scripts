using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private GameObject loadingUI;
    private GameObject currentLoadingUI;

    // ★ 중요: 통합된 게임 씬의 정확한 이름
    private const string GAME_SCENE_NAME = "GameScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadRecordMode()
    {
        // 1. 모드 설정 저장
        GameSettings.SelectedMode = GameMode.RecordHeight;
        // 2. 통합 씬 로드
        LoadSceneWithLoading(GAME_SCENE_NAME);
    }

    public void LoadMaxBlocksMode()
    {
        GameSettings.SelectedMode = GameMode.MaxBlock;
        LoadSceneWithLoading(GAME_SCENE_NAME);
    }

    public void LoadSprintMode() // 이름 변경: LoadTimeMode -> LoadSprintMode
    {
        GameSettings.SelectedMode = GameMode.Sprint; // Enum 변경
        LoadSceneWithLoading("GameScene");
    }

    public void LoadMainMenu()
    {
        GameSettings.SelectedMode = GameMode.None;
        LoadSceneWithLoading("MainMenu");
    }

    public void LoadSceneWithLoading(string targetScene)
    {
        StartCoroutine(LoadSceneAsync(targetScene));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingUI != null && currentLoadingUI == null)
        {
            currentLoadingUI = Instantiate(loadingUI);
            DontDestroyOnLoad(currentLoadingUI);
        }
        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (currentLoadingUI != null)
        {
            Destroy(currentLoadingUI);
            currentLoadingUI = null;
        }
    }
}