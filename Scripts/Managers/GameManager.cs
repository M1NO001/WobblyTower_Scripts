using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

// --- [데이터 구조체] ---
public enum GameMode { None, RecordHeight, MaxBlock, Sprint }

[System.Serializable]
public struct BlockData
{
    public GameObject blockPrefab;
    [Range(1, 100)] public int weight;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- [참조 및 프리팹] ---
    [Header("References")]
    public GameObject[] startingPanel;
    public GameObject previewStructurePrefab;

    [HideInInspector] public BlockController activeBlockController;
    [HideInInspector] public GameObject activeBlock;

    private GameObject currentBlock;

    // 높이 기록용
    public float CurrentHighestY = -3f;

    // --- [블록 설정] ---
    [Header("Block Config")]
    public BlockData[] blockPatterns; // 이 리스트의 순서대로 색깔이 배정됩니다.

    [System.Serializable]
    public struct QueuedBlock { public GameObject prefab; public Color color; }
    private List<QueuedBlock> nextBlockQueue = new List<QueuedBlock>();
    private List<BlockController> managedBlocks = new List<BlockController>();

    // --- [앵커] ---
    [Header("Anchors")]
    public Transform[] previewPositions;
    [SerializeField] private Transform spawnAnchor;
    public Vector2 previewPadding = new Vector2(2.5f, 3.0f);
    private PreviewAnchor[] previewAnchors = new PreviewAnchor[10];

    // --- [게임 설정] ---
    [Header("Game Settings")]
    public GameMode currentMode = GameMode.None;
    public bool IsGameOver { get; private set; } = false;

    // --- [목숨] ---
    [Header("Life System")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int lives = 3;
    [SerializeField] private float damageCooldown = 2.0f;
    private float lastDamageTime = -999f;

    // --- [기록] ---
    [Header("Score & HUD")]
    [SerializeField] private float sessionMaxHeight = 0f;
    private float baselineY = 0f;
    [SerializeField] private InGameHud inGameHud;

    [Header("MaxBlocks Mode Config")]
    [SerializeField] private int maxBlocksGoal = 40;
    private int usedBlocks = 0;

    // --- [UI] ---
    [Header("UI")]
    [SerializeField] private GameObject gameOverCanvasPrefab;
    private GameObject gameOverCanvasInstance;
    private GameOverCanvasBinder gameOverBinder;
    private bool isPaused = false;

    [Header("Sprint Config")]
    public float targetHeight = 20f;
    private float elapsedTime = 0f;

    public Vector2 startingPanelSpawnPosition = new Vector2(0, -3);

    // ★ [수정] 고정 색상 팔레트 (순서대로 블록에 적용됨)
    private readonly string[] fixedColors = new string[] {
        "#FF6B6B", // 0번 블록 색 (빨강 계열)
        "#4ECDC4", // 1번 블록 색 (민트 계열)
        "#FFE66D", // 2번 블록 색 (노랑 계열)
        "#1A535C", // 3번 블록 색 (청록 계열)
        "#FF9F1C", // 4번 블록 색 (오렌지 계열)
        "#95E1D3", // 5번 블록 색 (연한 민트)
        "#F38181", // 6번 블록 색 (분홍 계열)
        "#A8D8EA", // 7번 블록 색 (하늘 계열)
        "#AA96DA", // 8번 블록 색 (보라 계열)
        "#B8F2E6", // 9번...
        "#FFD3B6",
        "#FFAAA5"
    };

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;
        if (GameSettings.SelectedMode != GameMode.None) StartCoroutine(SetupAfterSceneLoad());
        else InitGame(GameMode.RecordHeight);
    }

    void Update()
    {
        // ★ [추가] 뒤로가기(ESC) 키를 누르면 일시정지 토글
        // (메인 메뉴가 아니고, 게임 오버 상태가 아닐 때만)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "MainMenu" && !IsGameOver)
            {
                TogglePause();
            }
        }
        if (IsGameOver) return;
        if (inGameHud == null) return;

        if (currentMode == GameMode.Sprint)
        {
            elapsedTime += Time.deltaTime;
            inGameHud.SetTime(elapsedTime, RecordStorage.LoadBestSprintTime());
            if (CurrentHighestY - baselineY >= targetHeight) GameClear();
        }
    }

    private void GameClear()
    {
        IsGameOver = true;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlayVictory();
        }

        bool isNewRecord = false;
        float bestTime = RecordStorage.LoadBestSprintTime();
        if (elapsedTime < bestTime)
        {
            isNewRecord = true;
            RecordStorage.SaveBestSprintTime(elapsedTime);
            bestTime = elapsedTime;
        }
        ShowGameOverUI(true, bestTime, isNewRecord);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") return;
        if (GameSettings.SelectedMode == GameMode.None) { SceneManager.LoadScene("MainMenu"); return; }
        StartCoroutine(SetupAfterSceneLoad());
    }

    private IEnumerator SetupAfterSceneLoad()
    {
        yield return null;

        // 앵커 동기화
        List<Transform> activePreviews = new List<Transform>();
        if (previewAnchors != null)
        {
            foreach (var anchor in previewAnchors)
            {
                if (anchor != null) activePreviews.Add(anchor.transform);
            }
        }
        previewPositions = activePreviews.ToArray();

        if (inGameHud != null)
        {
            inGameHud.ShowMode(GameSettings.SelectedMode);
            inGameHud.SetRecord(0f, RecordStorage.LoadBestHeight());
        }
        InitGame(GameSettings.SelectedMode);
    }

    public void InitGame(GameMode mode)
    {
        currentMode = mode;
        IsGameOver = false;

        baselineY = startingPanelSpawnPosition.y;
        CurrentHighestY = baselineY;
        sessionMaxHeight = 0f;
        lastDamageTime = -999f;
        Time.timeScale = 1f;

        HideGameOverUI();

        switch (mode)
        {
            case GameMode.RecordHeight: maxLives = 3; break;
            case GameMode.MaxBlock:
                maxLives = 3; usedBlocks = 0;
                if (inGameHud != null) inGameHud.SetMaxBlocks(usedBlocks, maxBlocksGoal);
                break;
            case GameMode.Sprint:
                maxLives = 3; elapsedTime = 0f;
                break;
        }

        AudioManager.Instance?.PlayInGameBGM();
        SpawnStartingPanel(startingPanel[0]);
        lives = maxLives;
        UpdateLivesUI();
        InitNextBlocks();
        SpawnNextBlock();
    }

    // --- ★ [수정] 랜덤 인덱스를 뽑는 함수 (블록 종류를 결정) ---
    private int GetRandomBlockIndex()
    {
        if (blockPatterns == null || blockPatterns.Length == 0) return -1;

        int totalWeight = 0;
        foreach (var data in blockPatterns) totalWeight += data.weight;

        int randomValue = Random.Range(0, totalWeight);
        for (int i = 0; i < blockPatterns.Length; i++)
        {
            if (randomValue < blockPatterns[i].weight) return i;
            randomValue -= blockPatterns[i].weight;
        }
        return 0;
    }

    // --- ★ [수정] 인덱스에 맞는 고정 색상을 가져오는 함수 ---
    private Color GetFixedColor(int index)
    {
        // 블록 인덱스를 색상 배열 길이로 나눈 나머지값 사용 (색상이 모자라면 처음 색부터 다시 씀)
        int colorIndex = index % fixedColors.Length;
        string hex = fixedColors[colorIndex];

        if (ColorUtility.TryParseHtmlString(hex, out Color newColor)) return newColor;
        return Color.white;
    }

    // --- ★ [수정] 초기 큐 생성 ---
    // --- [수정됨] 초기 큐 생성 ---
    public void InitNextBlocks()
    {
        nextBlockQueue.Clear();
        if (previewPositions == null || previewPositions.Length == 0) return;

        for (int i = 0; i < previewPositions.Length; i++)
        {
            if (previewPositions[i] == null) continue;

            // 1. 랜덤 인덱스 뽑기
            int blockIndex = GetRandomBlockIndex();
            if (blockIndex == -1) continue;

            // 2. 해당 인덱스의 프리팹과 고정 색상 가져오기
            QueuedBlock qb = new QueuedBlock();
            qb.prefab = blockPatterns[blockIndex].blockPrefab;
            qb.color = GetFixedColor(blockIndex);

            if (qb.prefab == null) continue;
            nextBlockQueue.Add(qb);

            // 기존 프리뷰 삭제
            if (previewPositions[i].childCount > 0) Destroy(previewPositions[i].GetChild(0).gameObject);

            // 새 프리뷰 생성
            GameObject previewBlock = Instantiate(qb.prefab, previewPositions[i]);

            // 프리뷰 설정 (위치, 크기)
            previewBlock.transform.localPosition = Vector3.zero;
            previewBlock.transform.localRotation = Quaternion.identity;
            previewBlock.transform.localScale = Vector3.one * 0.6f;

            // ★ [핵심 수정] 물리, 스크립트, 그리고 "콜라이더"까지 싹 제거!
            if (previewBlock.GetComponent<Rigidbody2D>()) Destroy(previewBlock.GetComponent<Rigidbody2D>());
            if (previewBlock.GetComponent<BlockController>()) Destroy(previewBlock.GetComponent<BlockController>());

            // ↓↓↓ 이 부분이 추가되었습니다! (모든 충돌체 제거)
            Collider2D[] colliders = previewBlock.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders) Destroy(col);
            // ↑↑↑

            // 색상 적용
            SpriteRenderer[] renderers = previewBlock.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in renderers) sr.color = qb.color;
        }
    }

    // --- ★ [수정] 다음 블록 생성 ---
    // --- [수정됨] 다음 블록 생성 ---
    public void SpawnNextBlock()
    {
        if (currentBlock != null) return;
        if (nextBlockQueue.Count == 0) return;

        QueuedBlock qb = nextBlockQueue[0];
        nextBlockQueue.RemoveAt(0);

        Vector3 spawnPos = (spawnAnchor != null) ? spawnAnchor.position : new Vector3(0, 5, 0);
        currentBlock = Instantiate(qb.prefab, spawnPos, Quaternion.identity);

        BlockController bc = currentBlock.GetComponent<BlockController>();
        if (bc != null) bc.SetColor(qb.color);

        // 다음 블록 채워넣기 (새로운 블록 뽑기)
        int nextIndex = GetRandomBlockIndex();
        QueuedBlock newQb = new QueuedBlock();
        newQb.prefab = blockPatterns[nextIndex].blockPrefab;
        newQb.color = GetFixedColor(nextIndex);
        nextBlockQueue.Add(newQb);

        // 프리뷰 갱신
        if (previewPositions != null)
        {
            for (int i = 0; i < previewPositions.Length; i++)
            {
                if (i >= nextBlockQueue.Count) break;
                if (previewPositions[i] == null) continue;

                if (previewPositions[i].childCount > 0) Destroy(previewPositions[i].GetChild(0).gameObject);

                QueuedBlock nextInfo = nextBlockQueue[i];
                GameObject previewBlock = Instantiate(nextInfo.prefab, previewPositions[i]);

                previewBlock.transform.localPosition = Vector3.zero;
                previewBlock.transform.localRotation = Quaternion.identity;
                previewBlock.transform.localScale = Vector3.one * 0.6f;

                // ★ [핵심 수정] 여기서도 콜라이더 싹 제거!
                if (previewBlock.GetComponent<Rigidbody2D>()) Destroy(previewBlock.GetComponent<Rigidbody2D>());
                if (previewBlock.GetComponent<BlockController>()) Destroy(previewBlock.GetComponent<BlockController>());

                // ↓↓↓ 추가됨
                Collider2D[] colliders = previewBlock.GetComponentsInChildren<Collider2D>();
                foreach (var col in colliders) Destroy(col);
                // ↑↑↑

                SpriteRenderer[] renderers = previewBlock.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in renderers) sr.color = nextInfo.color;
            }
        }

        Rigidbody2D rb = currentBlock.GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        currentBlock.tag = "Block"; activeBlock = currentBlock; activeBlockController = bc;

        if (currentMode == GameMode.MaxBlock) { usedBlocks++; inGameHud?.SetMaxBlocks(usedBlocks, maxBlocksGoal); }
    }

    // --- 나머지 함수들은 동일 ---
    public void RegisterPreviewAnchor(PreviewAnchor anchor)
    {
        if (anchor == null) return;
        int idx = anchor.index;
        if (idx >= 0 && idx < previewAnchors.Length) previewAnchors[idx] = anchor;
    }
    public void RegisterSpawnAnchor(SpawnAnchor anchor) { if (anchor != null) spawnAnchor = anchor.transform; }
    public void RegisterBlock(BlockController block) { if (!managedBlocks.Contains(block)) managedBlocks.Add(block); }
    public void UnregisterBlock(BlockController block) { if (managedBlocks.Contains(block)) managedBlocks.Remove(block); }
    public void CreateBlock() { if (IsGameOver) return; SpawnNextBlock(); }
    public void SetActiveBlock(BlockController block) => activeBlockController = block;
    public void MoveLeft() { if (!IsGameOver) activeBlockController?.MoveLeft(); }
    public void MoveRight() { if (!IsGameOver) activeBlockController?.MoveRight(); }
    public void StopHorizontal() { if (!IsGameOver) activeBlockController?.StopHorizontal(); }
    public void RotateBlock() { if (!IsGameOver) activeBlockController?.Rotate(); }
    public void TogglePause() { if (IsGameOver) return; isPaused = !isPaused; Time.timeScale = isPaused ? 0f : 1f; inGameHud?.SetPausePanel(isPaused); }
    public void ResumeGame() { if (isPaused) TogglePause(); }
    public void ReturnToMainMenu() { Time.timeScale = 1f; AudioManager.Instance?.PlayMainMenuBGM(); GameSettings.SelectedMode = GameMode.None; SceneManager.LoadScene("MainMenu"); }
    public void RestartRecordMode() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    private void UpdateLivesUI() => inGameHud?.UpdateLives(lives);

    public void UpdateHighestHeight(float newHeight)
    {
        if (newHeight > CurrentHighestY)
        {
            CurrentHighestY = newHeight;
            if (currentMode == GameMode.RecordHeight)
            {
                float relativeHeight = CurrentHighestY - baselineY;
                if (relativeHeight > sessionMaxHeight)
                {
                    sessionMaxHeight = relativeHeight;
                    inGameHud?.SetRecord(sessionMaxHeight, RecordStorage.LoadBestHeight());
                    if (sessionMaxHeight > RecordStorage.LoadBestHeight()) RecordStorage.SaveBestHeight(sessionMaxHeight);
                }
            }
        }
    }

    public void NotifyBlockStable(BlockController block)
    {
        RefreshCurrentHeight();
        if (currentMode == GameMode.RecordHeight)
        {
            float relativeHeight = CurrentHighestY - baselineY;
            if (relativeHeight > sessionMaxHeight)
            {
                sessionMaxHeight = relativeHeight;
                inGameHud?.SetRecord(sessionMaxHeight, RecordStorage.LoadBestHeight());
                if (sessionMaxHeight > RecordStorage.LoadBestHeight()) RecordStorage.SaveBestHeight(sessionMaxHeight);
            }
        }
    }

    public void RefreshCurrentHeight()
    {
        float highest = startingPanelSpawnPosition.y;
        foreach (var block in managedBlocks)
        {
            if (block == null || !block.IsLanded) continue;
            Collider2D[] allColliders = block.GetComponentsInChildren<Collider2D>();
            foreach (var col in allColliders) { if (col != null) { float topY = col.bounds.max.y; if (topY > highest) highest = topY; } }
        }
        CurrentHighestY = highest;
    }

    public void OnFallTriggerHit(BlockController block)
    {
        if (IsGameOver || block == null) return;
        if (Time.time - lastDamageTime < damageCooldown)
        {
            if (block.gameObject != null) Destroy(block.gameObject);
            Invoke(nameof(RefreshCurrentHeight), 0.1f);
            return;
        }

        lastDamageTime = Time.time;
        lives--;
        UpdateLivesUI();

        Destroy(block.gameObject);
        RefreshCurrentHeight();

        if (block == activeBlockController) { activeBlock = null; activeBlockController = null; currentBlock = null; }

        if (lives <= 0) GameOver();
        else if (currentBlock == null) CreateBlock();
    }

    private void GameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0f;

        bool isNewRecord = false;
        float bestScore = 0f; // 상황에 따라 높이일 수도, 시간일 수도 있음

        // ★ [수정됨] 모드별로 다른 최고 기록을 가져오도록 변경
        if (currentMode == GameMode.Sprint)
        {
            // 스프린트 모드: 최고 시간 기록 가져오기
            bestScore = RecordStorage.LoadBestSprintTime();

            // 스프린트에서 죽으면(Game Over) 어차피 실패니까 신기록 갱신은 없음 (성공해야만 갱신)
            isNewRecord = false;
        }
        else if (currentMode == GameMode.RecordHeight)
        {
            // 높이 모드: 최고 높이 기록 가져오기
            float currentBest = RecordStorage.LoadBestHeight();

            // 신기록인지 판별
            if (sessionMaxHeight > currentBest)
            {
                isNewRecord = true;
                bestScore = sessionMaxHeight; // 신기록이면 현재 기록을 보여줌
                // (저장은 이미 UpdateHighestHeight에서 했겠지만 안전하게)
                RecordStorage.SaveBestHeight(sessionMaxHeight);
            }
            else
            {
                bestScore = currentBest;
            }
        }
        else if (currentMode == GameMode.MaxBlock)
        {
            // (혹시 나중에 구현할 맥스블록 모드용)
            bestScore = 0; // 임시
        }

        // 사운드 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            if (isNewRecord) AudioManager.Instance.PlayVictory();
            else AudioManager.Instance.PlayGameOver();
        }

        // UI 표시 (이제 bestScore에 올바른 값이 들어갑니다)
        ShowGameOverUI(false, bestScore, isNewRecord);

        if (AdMobManager.Instance != null)
        {
            AdMobManager.Instance.ShowAd();
        }
    }

    private void ShowGameOverUI(bool isClear, float bestScore, bool isNewRecord)
    {
        if (gameOverCanvasInstance == null && gameOverCanvasPrefab != null)
        {
            gameOverCanvasInstance = Instantiate(gameOverCanvasPrefab);
            gameOverBinder = gameOverCanvasInstance.GetComponent<GameOverCanvasBinder>();
        }

        if (gameOverBinder != null)
        {
            gameOverBinder.ShowMode(currentMode);
            string title = isClear ? "MISSION COMPLETE" : (isNewRecord ? "NEW RECORD!" : "GAME OVER");
            gameOverBinder.SetTitle(title);

            if (currentMode == GameMode.Sprint)
                gameOverBinder.BindTime(elapsedTime, bestScore, isNewRecord);
            else if (currentMode == GameMode.MaxBlock)
                gameOverBinder.BindMaxBlocks(usedBlocks, maxBlocksGoal, isNewRecord);
            else
                gameOverBinder.BindRecord(sessionMaxHeight, bestScore, isNewRecord);
        }
        if (gameOverCanvasInstance != null) gameOverCanvasInstance.SetActive(true);
    }

    private void HideGameOverUI() { if (gameOverCanvasInstance != null) gameOverCanvasInstance.SetActive(false); }

    public void BindHud(InGameHud hud) => inGameHud = hud;
    public Vector3 GetSpawnPosition() => (spawnAnchor != null) ? spawnAnchor.position : Vector3.zero;

    public void OnBlockLanded()
    {
        activeBlock = null; activeBlockController = null; currentBlock = null;
        Invoke(nameof(CreateBlock), 0.1f);
    }
    private void SpawnStartingPanel(GameObject prefab) { if (prefab != null) Instantiate(prefab, startingPanelSpawnPosition, Quaternion.identity); }
}