using UnityEngine;
using TMPro;

public class InGameHud : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BindHud(this);
        }
    }

    [Header("Roots (모드별 UI 루트)")]
    [SerializeField] private GameObject recordRoot;
    [SerializeField] private GameObject maxBlocksRoot;
    [SerializeField] private GameObject timeRoot;

    [Header("Record Mode")]
    [SerializeField] private TMP_Text recordCurrentText;
    [SerializeField] private TMP_Text recordBestText;

    [Header("MaxBlocks Mode")]
    [SerializeField] private TMP_Text maxCurrentText;
    [SerializeField] private TMP_Text maxGoalText;

    [Header("Time Mode")]
    [SerializeField] private TMP_Text timeCurrentText;
    [SerializeField] private TMP_Text timeBestText;

    [Header("Common UI (목숨 & 일시정지)")]
    [SerializeField] private TMP_Text livesText;      // 하트 옆 숫자 (x 3)
    [SerializeField] private GameObject pauseMenuPanel; // 일시정지 패널

    public void ShowMode(GameMode mode)
    {
        recordRoot?.SetActive(mode == GameMode.RecordHeight);
        maxBlocksRoot?.SetActive(mode == GameMode.MaxBlock);
        timeRoot?.SetActive(mode == GameMode.Sprint);
    }

    // === Record ===
    public void SetRecord(float current, float best)
    {
        if (recordCurrentText) recordCurrentText.text = $"Height {current:F2}m";
        if (recordBestText) recordBestText.text = $"Best {best:F2}m";
    }

    // === MaxBlocks ===

    public void SetMaxBlocks(int current, int goal)
    {
        if (maxCurrentText) maxCurrentText.text = $"Used : {current}";
        if (maxGoalText) maxGoalText.text = $"Goal : {goal}";
    }

    // === Time ===
    public void SetTime(float elapsed, float best)
    {
        if (timeCurrentText) timeCurrentText.text = FormatTime(elapsed);
        if (timeBestText) timeBestText.text = $"Best {FormatTime(best)}";
    }

    private string FormatTime(float t)
    {
        int min = Mathf.FloorToInt(t / 60f);
        float sec = t - min * 60f;
        return $"{min:00}:{sec:00.00}";
    }
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            if (lives >= 100) livesText.text = "∞";
            else livesText.text = $"x {lives}";
        }
    }

    // ★★★ 2. 일시정지 패널 켜고 끄기 ★★★
    public void SetPausePanel(bool isActive)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(isActive);
    }

    // ★★★ 3. 버튼 연결용 함수들 (버튼 클릭 시 이걸 연결하세요) ★★★
    // 버튼들이 GameManager를 찾으러 다닐 필요 없이, 바로 옆에 있는 이 함수들을 부르면 됩니다.
    public void OnClickPause() => GameManager.Instance.TogglePause();
    public void OnClickResume() => GameManager.Instance.ResumeGame();
    public void OnClickRestart() => GameManager.Instance.RestartRecordMode();
    public void OnClickHome() => GameManager.Instance.ReturnToMainMenu();
}
