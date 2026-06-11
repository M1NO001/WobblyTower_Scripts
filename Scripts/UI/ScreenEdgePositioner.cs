using UnityEngine;

public class ScreenEdgePositioner : MonoBehaviour
{
    private enum HorizontalAlign { Center, Left, Right }

    [Header("Settings")]
    [SerializeField] private HorizontalAlign align = HorizontalAlign.Right; // 오른쪽 정렬이 기본
    [SerializeField] private float padding = 1.5f; // 가장자리에서 얼마나 떨어뜨릴지 (안쪽으로)

    void Start()
    {
        AdjustPosition();
    }

    // 화면 해상도가 바뀔 때(에디터에서) 대응하려면 Update나 OnValidate에서도 쓸 수 있음
    // 모바일은 해상도가 안 바뀌니 Start면 충분함
    void AdjustPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. 카메라의 현재 세로 절반 크기 (Orthographic Size)
        float halfHeight = cam.orthographicSize;

        // 2. 비율을 곱해서 가로 절반 크기를 구함 (이게 화면의 오른쪽 끝 X좌표)
        float halfWidth = halfHeight * cam.aspect;

        float newX = 0f;

        // 3. 정렬 기준에 따라 X좌표 계산
        switch (align)
        {
            case HorizontalAlign.Right:
                // 오른쪽 끝(halfWidth)에서 padding만큼 안쪽(-)으로
                newX = halfWidth - padding;
                break;
            case HorizontalAlign.Left:
                // 왼쪽 끝(-halfWidth)에서 padding만큼 안쪽(+)으로
                newX = -halfWidth + padding;
                break;
            case HorizontalAlign.Center: // ★ [수정됨] 여기 에러 수정!
                newX = 0f;
                break;
        }

        // 4. 적용 (Y와 Z는 건드리지 않고 X만 바꿈)
        Vector3 newPos = transform.position;
        newPos.x = newX;
        transform.position = newPos;
    }
}