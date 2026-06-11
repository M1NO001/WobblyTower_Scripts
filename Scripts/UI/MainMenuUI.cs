using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button recordModeButton;
    public Button maxBlocksModeButton;
    public Button sprintModeButton;
    public Button exitButton;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMainMenuBGM();
        }
        // 버튼에 클릭 이벤트 등록
        recordModeButton.onClick.AddListener(() =>
        {
            SceneTransitionManager.Instance.LoadRecordMode();
        });

        maxBlocksModeButton.onClick.AddListener(() =>
        {
            SceneTransitionManager.Instance.LoadMaxBlocksMode();
        });

        sprintModeButton.onClick.AddListener(() =>
        {
            SceneTransitionManager.Instance.LoadSprintMode();
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서도 종료
#endif
        });
    }
}
