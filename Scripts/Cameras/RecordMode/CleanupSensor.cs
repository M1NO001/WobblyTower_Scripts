// Assets/Scripts/Sensors/CleanupSensor.cs
using UnityEngine;

public class CleanupSensor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        BlockController block = other.GetComponentInParent<BlockController>();
        if (block == null) return;

        // 무조건 제거만 (라이프 감소 없음)
        Destroy(block.gameObject);
    }
}
