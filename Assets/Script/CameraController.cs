using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Camera Settings")]
    public Transform target; // 카메라가 따라갈 대상
    public float rotationSpeed = 5f; // 마우스 회전 속도
    public float zoomSpeed = 8f; // 마우스 줌 속도
    public float minZoom = 2f; // 최소 줌 거리
    public float maxZoom = 15f; // 최대 줌 거리

    [Header("Camera Position")]
    public float currentZoom = 8; // 현재 줌 값

    private float mouseX = 0f; // 시점 변경을 위한 마우스 X축 회전 값
    private float mouseY = 0f; // 시점 변경을 위한 마우스 Y축 회전 값

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 타겟이 지정되지 않은 경우 첫 번째 로봇 자동 탐색
        if (target == null)
        {
            RobotController robot = FindObjectOfType<RobotController>();
            if (robot != null)
            {
                target = robot.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput(); // 줌/회전 등 입력 처리
        UpdateCameraPosition(); // 카메라 위치 계산 및 이동
        UpdateCameraRotation(); // 카메라가 대상 바라보도록 회전
    }

    /// <summary>
    /// 마우스 회전, 줌 입력 처리
    /// </summary>
    private void HandleInput()
    {
        // 오른쪽 마우스 버튼을 누르면 카메라 회전
        if (Input.GetMouseButton(1)) 
        {
            mouseX += Input.GetAxis("Mouse X") * rotationSpeed; // 좌우 회전
            mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed; // 상하 회전 (마우스 Y 반전)
            mouseY = Mathf.Clamp(mouseY, -30f, 60f); // 위아래 회전 제한
        }

        // 마우스 휠로 줌 인/아웃
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom); // 줌 범위 제한
    }

    /// <summary>
    /// 카메라의 위치를 계산하여 대상 주변으로 이동
    /// </summary>
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0); // 상하좌우 회전 생성
        Vector3 rotatedOffset = rotation * (Vector3.back * currentZoom + Vector3.up * 2f); // 회전된 줌 오프셋
        Vector3 targetPosition = target.position + rotatedOffset; // 타겟 기준 위치 계산

        // 카메라 이동
        transform.position = targetPosition;
    }

    /// <summary>
    /// 카메라가 대상 바라보도록 회전
    /// </summary>
    private void UpdateCameraRotation()
    {
        // 타겟 방향 계산
        Vector3 direction = target.position - transform.position;

        if (direction != Vector3.zero)
        {
            // 즉시 대상 방향으로 회전
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// 새로운 타겟 설정 (예: 로봇 선택 시 호출됨)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 카메라 회전 및 줌 상태 초기화
    /// </summary>
    public void ResetCamera()
    {
        mouseX = 0f;
        mouseY = 0f;
        currentZoom = 5f;
    }

    /// <summary>
    /// 카메라가 바라보는 방향의 평면상 전방 벡터 반환
    /// (로봇 이동 등에 활용 가능)
    /// </summary>
    public Vector3 GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f; // 수평 방향만 유지
        return forward.normalized;
    }

    /// <summary>
    /// 카메라 기준 평면상의 오른쪽 방향 벡터 반환
    /// </summary>
    public Vector3 GetRightDirection()
    {
        Vector3 right = transform.right;
        right.y = 0f; // 수평 방향만 유지
        return right.normalized;
    }
}