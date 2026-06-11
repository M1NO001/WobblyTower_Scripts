using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Direction { Left, Right }
    public Direction direction;

    private bool isHeld = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
        Move();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        GameManager.Instance.StopHorizontal();
    }

    void Update()
    {
        if (isHeld)
        {
            Move();
        }
    }

    void Move()
    {
        if (GameManager.Instance.activeBlockController != null)
        {
            if (direction == Direction.Left)
                GameManager.Instance.MoveLeft();
            else if (direction == Direction.Right)
                GameManager.Instance.MoveRight();
        }
    }
}
