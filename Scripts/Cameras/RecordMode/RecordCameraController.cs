using UnityEngine;

public class RecordCameraController : MonoBehaviour
{
    [Header("Settings")]
    public float topMargin = 2f;    // 화면 위쪽 여백
    public float smoothTime = 0.3f; // 이동 부드러움 정도
    public float minCameraY = 0f;   // 카메라 최저 높이

    private Camera cam;
    private float currentVelocity;

    // 앵커(벽, 배경) 이동
    [Header("Anchors")]
    public Transform anchorsRoot;
    private Vector3 anchorsOffset;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 앵커와 카메라의 간격(Offset) 기억
        if (anchorsRoot != null)
        {
            anchorsOffset = anchorsRoot.position - transform.position;
        }

        minCameraY = transform.position.y;
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null) return;

        // ★ [핵심] FindObject 없이 매니저의 변수를 바로 가져옴 (성능 최적화)
        float currentTowerTop = GameManager.Instance.CurrentHighestY;

        float camHalfHeight = cam.orthographicSize;

        // 목표 카메라 Y 위치 계산
        float targetCamY = currentTowerTop - camHalfHeight + topMargin;

        // 바닥 제한
        targetCamY = Mathf.Max(targetCamY, minCameraY);

        // 부드럽게 이동
        float newY = Mathf.SmoothDamp(transform.position.y, targetCamY, ref currentVelocity, smoothTime);

        // 이동 적용
        Vector3 newPos = transform.position;
        newPos.y = newY;
        transform.position = newPos;

        // ★ 앵커 이동 (카메라 따라가기)
        if (anchorsRoot != null)
        {
            anchorsRoot.position = transform.position + anchorsOffset;
        }
    }
}