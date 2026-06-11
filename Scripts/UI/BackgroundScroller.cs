using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Settings")]
    public Transform cameraTransform; // 메인 카메라 연결
    [Range(0f, 1f)]
    public float parallaxSpeed = 0.5f; // 0이면 배경 고정, 1이면 배경이 카메라랑 똑같이 움직임(안 움직이는 것처럼 보임)

    private MeshRenderer meshRenderer;
    private float initialOffsetY;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // 카메라를 안 넣었으면 자동으로 찾음
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. 배경 판때기(Quad)는 무조건 카메라를 따라다닙니다. (항상 화면 중앙에 위치)
        transform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, transform.position.z);

        // 2. 대신 그림(Texture)의 위치(Offset)를 카메라 높이에 비례해서 밀어줍니다.
        // Y축으로만 스크롤 (카메라 Y * 속도)
        float offset = cameraTransform.position.y * parallaxSpeed * 0.1f; // 0.1은 민감도 조절용

        // 머티리얼의 오프셋을 변경해서 그림을 이동시킴
        meshRenderer.material.mainTextureOffset = new Vector2(0, offset);
    }
}