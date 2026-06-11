using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public float smoothSpeed = 2f;   // 따라가는 속도
    public float yOffset = 2f;       // 최고 높이보다 얼마나 위에 있을지
    public float minY = 0f;          // 카메라 바닥 제한 (초기 위치)

    private float currentHighestY;   // 현재 목표 높이

    void Start()
    {
        currentHighestY = minY;
    }

    void LateUpdate()
    {
        // 1. 착지한 놈들 중에서만 제일 높은 놈 찾기
        CalculateHighestLandedBlock();

        // 2. 목표 높이 설정 (바닥 밑으로는 안 내려감)
        float targetY = Mathf.Max(currentHighestY + yOffset, minY);

        // 3. 부드럽게 이동
        Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    void CalculateHighestLandedBlock()
    {
        // "Block" 태그가 붙은 모든 오브젝트 검색
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");

        float maxY = minY;

        foreach (GameObject obj in blocks)
        {
            // ★ 핵심 수정: BlockController를 가져와서 '착지했는지' 확인
            BlockController block = obj.GetComponent<BlockController>();

            // 블록 스크립트가 있고 + "착지한 상태(IsLanded)"일 때만 계산
            // (변수명이 isLanded 소문자라면 block.isLanded 로 고쳐주세요!)
            if (block != null && block.IsLanded)
            {
                if (obj.transform.position.y > maxY)
                {
                    maxY = obj.transform.position.y;
                }
            }
        }

        // 높이는 계속 갱신 (떨어지면 카메라도 같이 내려가게 하려면 조건문 없이 대입)
        currentHighestY = maxY;
    }
}