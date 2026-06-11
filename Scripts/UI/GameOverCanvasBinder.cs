using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 필수
using UnityEngine.SceneManagement;

public class GameOverCanvasBinder : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text titleText;       // 제목 (Game Over)
    public TMP_Text mainScoreText;   // 가운데 큰 점수 (점수 + 칭호)
    public TMP_Text subScoreText;    // 아래 작은 최고기록

    [Header("★ 신기록 전용 오브젝트 (여기에 연결!)")]
    public GameObject newRecordUI;   // 신기록일 때만 켜질 왕관이나 축하 텍스트 그룹

    [Header("버튼")]
    public Button restartButton;
    public Button homeButton;

    [Header("설정 (색상 등)")]
    public Color newRecordColor = Color.yellow; // 신기록일 때 글자 색깔
    public Color normalColor = Color.white;     // 평소 글자 색깔

    void Start()
    {
        // 버튼 기능 자동 연결
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        }

        // 게임 시작할 때는 신기록 UI가 켜져있으면 안 되므로 일단 끔
        if (newRecordUI != null) newRecordUI.SetActive(false);
    }

    // --- 외부 호출 함수들 ---

    public void ShowMode(GameMode mode)
    {
        gameObject.SetActive(true);
    }

    public void SetTitle(string text)
    {
        if (titleText != null) titleText.text = text;
    }

    // [높이 모드] 칭호 시스템 + 신기록 배너
    public void BindRecord(float currentHeight, float bestHeight, bool isNew)
    {
        if (mainScoreText != null)
        {
            // 1. 칭호 가져오기
            string rankTitle = GetHeightRank(currentHeight);

            // 2. 텍스트 표시 (점수 + 줄바꿈 + 칭호)
            // 가독성을 위해 \n(줄바꿈)을 넣었습니다.
            mainScoreText.text = $"Height: {currentHeight:F2}m {rankTitle}";

            // 3. 신기록이면 글자 색깔 바꾸기
            mainScoreText.color = isNew ? newRecordColor : normalColor;
        }

        if (subScoreText != null)
        {
            subScoreText.text = $"Best: {bestHeight:F2}m";
        }

        // ★ 4. 신기록이면 축하 UI 켜주기!
        if (newRecordUI != null)
        {
            newRecordUI.SetActive(isNew);
        }
    }

    // [스피드런 모드]
    public void BindTime(float elapsedSec, float bestSec, bool isNew)
    {
        if (mainScoreText != null)
        {
            mainScoreText.text = $"Time: {FormatTime(elapsedSec)}";
            mainScoreText.color = isNew ? newRecordColor : normalColor;
        }

        if (subScoreText != null)
        {
            subScoreText.text = $"Best: {FormatTime(bestSec)}";
        }

        // ★ 4. 신기록이면 축하 UI 켜주기!
        if (newRecordUI != null)
        {
            newRecordUI.SetActive(isNew);
        }
    }

    // [칭호 판별기]
    private string GetHeightRank(float height)
    {
        // <color=색상>글자</color>  <- 색깔 바꾸기
        // <b>글자</b>               <- 굵게 만들기
        // <size=120%>글자</size>    <- 폰트 크기 키우기

        if (height < 10.0f)
            return "<color=#CCCCCC>Beginner</color>"; // 회색

        if (height < 30.0f)
            return "<color=#90EE90><b>Builder</b></color>"; // 연두색

        if (height < 60.0f)
            return "<color=#00FFFF><size=110%><b>Master</b></size></color>"; // 형광 하늘색

        if (height < 100.0f)
            return "<color=#FF00FF><size=120%><b>Sky Walker</b></size></color>"; // 핫핑크

        // 100m 이상
        return "<color=#FFD700><size=130%><b> GOD </b></size></color>";
    }

    // 시간 포맷 변환
    private string FormatTime(float t)
    {
        if (t > 900000f) return "--:--";
        int min = Mathf.FloorToInt(t / 60f);
        float sec = t % 60f;
        return $"{min:00}:{sec:00.00}";
    }

    // (기타 호환용 함수)
    public void BindMaxBlocks(int used, int goal, bool isNew)
    {
        if (mainScoreText) mainScoreText.text = $"{used} / {goal}";
    }
}