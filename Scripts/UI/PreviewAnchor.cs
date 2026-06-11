using UnityEngine;

public class PreviewAnchor : MonoBehaviour
{
    [Range(0, 9)]
    public int index;

    void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPreviewAnchor(this);
    }
}
