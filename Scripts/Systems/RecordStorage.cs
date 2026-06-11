using UnityEngine;

public static class RecordStorage
{
    private const string KEY_BEST_HEIGHT = "RecordHeight.BestHeight";
    private const string KEY_BEST_AT = "RecordHeight.BestAtUtc";
    private const string KEY_BEST_SPRINT = "Sprint.BestTime";
    public static float LoadBestHeight() =>
        PlayerPrefs.GetFloat(KEY_BEST_HEIGHT, 0f);

    public static void SaveBestHeight(float value)
    {
        float prev = LoadBestHeight();
        if (value <= prev) return;

        PlayerPrefs.SetFloat(KEY_BEST_HEIGHT, value);
        PlayerPrefs.SetString(KEY_BEST_AT, System.DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
    }

    public static string LoadBestWhenIsoUtc() => PlayerPrefs.GetString(KEY_BEST_AT, "");
    public static float LoadBestSprintTime()
    {
        // 기록이 없으면 아주 큰 값(999999)을 반환해서,
        // 첫 플레이 시 무조건 신기록이 되게 함
        return PlayerPrefs.GetFloat(KEY_BEST_SPRINT, 5999.99f);
    }

    // 스피드런 기록 저장하기
    public static void SaveBestSprintTime(float newTime)
    {
        float currentBest = LoadBestSprintTime();

        // 더 빨리 깼을 때만 저장 (작을수록 좋음)
        if (newTime < currentBest)
        {
            PlayerPrefs.SetFloat(KEY_BEST_SPRINT, newTime);
            PlayerPrefs.Save();
        }
    }
}
