using UnityEngine;
using System.Collections; // ★ 이거 꼭 추가하세요! (IEnumerator 쓰려면 필요)

[RequireComponent(typeof(LineRenderer))]
public class GoalLineRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float lineWidth = 0.1f;
    public Color lineColor = Color.green;
    public float lineLength = 20f;

    public Material lineMaterial;

    private LineRenderer lr;

    // ★ void Start() -> IEnumerator Start() 로 변경!
    IEnumerator Start()
    {
        // ★ GameManager가 초기화될 때까지 1프레임 기다림 (이게 핵심!)
        yield return null;

        if (GameManager.Instance == null) yield break;

        // 스프린트 모드가 아니면 선 끄기
        if (GameManager.Instance.currentMode != GameMode.Sprint)
        {
            gameObject.SetActive(false);
            yield break; // 함수 종료
        }

        lr = GetComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        // 머티리얼 설정
        if (lineMaterial != null) lr.material = lineMaterial;
        else lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.positionCount = 2;

        // 목표 높이 계산
        float floorY = GameManager.Instance.startingPanelSpawnPosition.y;
        float targetY = GameManager.Instance.targetHeight + floorY;

        // 선 그리기
        transform.position = new Vector3(0, targetY, 0);
        Vector3 startPos = new Vector3(-lineLength / 2, targetY, 0);
        Vector3 endPos = new Vector3(lineLength / 2, targetY, 0);

        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
    }
}