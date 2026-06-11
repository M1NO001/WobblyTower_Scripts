using UnityEngine;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float normalFallSpeed = -1f;
    public float acceleratedFallSpeed = -2f;

    private float currentFallSpeed;
    private Rigidbody2D rb;
    private bool hasLanded = false;
    private bool isAccelerating = false;
    public bool IsLanded => hasLanded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 5f;
        currentFallSpeed = normalFallSpeed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetActiveBlock(this);
            GameManager.Instance.RegisterBlock(this);
        }
    }

    void FixedUpdate()
    {
        if (!hasLanded && rb != null)
        {
            float clampedY = Mathf.Max(rb.linearVelocity.y, currentFallSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedY);
        }
    }

    public void SetColor(Color color)
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in allRenderers) sr.color = color;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return;

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Block"))
        {
            hasLanded = true;

            AudioManager.Instance?.PlayPlaceBlock();

            rb.gravityScale = 0.5f;
            StopHorizontal();
            StopAccel();
            currentFallSpeed = float.NegativeInfinity;

            // ★ [추가] 매니저에게 내 높이를 즉시 신고 (최적화 & 반응성)
            if (GameManager.Instance != null)
            {
                // 내 머리 위 높이 계산
                float myTopY = transform.position.y;
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) myTopY = col.bounds.max.y;

                // 신고! (스프린트 모드에서도 이게 호출되어 카메라가 올라감)
                GameManager.Instance.UpdateHighestHeight(myTopY);

                GameManager.Instance.NotifyBlockStable(this);

                if (GameManager.Instance.activeBlock == gameObject)
                {
                    GameManager.Instance.OnBlockLanded();
                }
            }
        }
    }

    public bool IsMovingDown(float vYThreshold = 0.1f) => rb != null && rb.linearVelocity.y < -vYThreshold;

    public void ForceAnchor()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        hasLanded = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GameManager.Instance?.NotifyBlockStable(this);
    }

    public void MoveLeft() => rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
    public void MoveRight() => rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    public void StopHorizontal() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    public void Rotate() => transform.Rotate(0, 0, -90);
    public void StartAccel() { isAccelerating = true; currentFallSpeed = acceleratedFallSpeed; }
    public void StopAccel() { isAccelerating = false; currentFallSpeed = normalFallSpeed; }
    void OnDestroy() { if (GameManager.Instance != null) GameManager.Instance.UnregisterBlock(this); }
}