// 파일: RecordFallTrigger.cs
using UnityEngine;

public class RecordFallTrigger : MonoBehaviour
{
    private float lastTriggerTime = -999f;

    [Tooltip("트리거 자체 중복 방지(이벤트 스팸만 억제). 라이프 중복 감소는 GameManager가 책임짐.")]
    public float triggerCooldown = 0.2f;

    [Tooltip("이 속도보다 빠르게 아래로 움직이면 Falling으로 간주")]
    public float vYThreshold = 0.1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BlockController block = other.GetComponentInParent<BlockController>();
        if (block == null) return;

        // 스팸 방지
        if (Time.time - lastTriggerTime <= triggerCooldown) return;
        lastTriggerTime = Time.time;

        // 속도 기반 판정
        bool falling = block.IsMovingDown(vYThreshold);

        if (falling)
        {
            // ↓ 여기서 블록 삭제는 하지 않고, GameManager에 알림만 보냄
            GameManager.Instance.OnFallTriggerHit(block);
        }
        else
        {
            // 정지한 블록이 선에 스치면 강제 고정
            block.ForceAnchor();
        }
    }
}
