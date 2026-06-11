using UnityEngine;
using UnityEngine.EventSystems;

public class BlockRotateButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.RotateBlock();
    }
}
