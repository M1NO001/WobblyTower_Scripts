using UnityEngine;
using UnityEngine.EventSystems;

public class BlockAccelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isHeld = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
        GameManager.Instance.activeBlockController?.StartAccel();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        GameManager.Instance.activeBlockController?.StopAccel();
    }

    void Update()
    {
        // 블록이 바뀌었을 때도 가속 상태 반영
        if (isHeld && GameManager.Instance.activeBlockController != null)
        {
            GameManager.Instance.activeBlockController.StartAccel();
        }
    }
}

