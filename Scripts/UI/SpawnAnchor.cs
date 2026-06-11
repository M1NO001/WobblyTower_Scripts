using UnityEngine;

public class SpawnAnchor : MonoBehaviour
{
    void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterSpawnAnchor(this);
    }
}
