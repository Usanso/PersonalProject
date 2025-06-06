using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Camera Settings")]
    public Transform target; // ī�޶� ���� ���
    public float rotationSpeed = 5f; // ���콺 ȸ�� �ӵ�
    public float zoomSpeed = 8f; // ���콺 �� �ӵ�
    public float minZoom = 2f; // �ּ� �� �Ÿ�
    public float maxZoom = 15f; // �ִ� �� �Ÿ�

    [Header("Camera Position")]
    public float currentZoom = 8; // ���� �� ��

    private float mouseX = 0f; // ���� ������ ���� ���콺 X�� ȸ�� ��
    private float mouseY = 0f; // ���� ������ ���� ���콺 Y�� ȸ�� ��

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
        // Ÿ���� �������� ���� ��� ù ��° �κ� �ڵ� Ž��
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

        HandleInput(); // ��/ȸ�� �� �Է� ó��
        UpdateCameraPosition(); // ī�޶� ��ġ ��� �� �̵�
        UpdateCameraRotation(); // ī�޶� ��� �ٶ󺸵��� ȸ��
    }

    /// <summary>
    /// ���콺 ȸ��, �� �Է� ó��
    /// </summary>
    private void HandleInput()
    {
        // ������ ���콺 ��ư�� ������ ī�޶� ȸ��
        if (Input.GetMouseButton(1)) 
        {
            mouseX += Input.GetAxis("Mouse X") * rotationSpeed; // �¿� ȸ��
            mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed; // ���� ȸ�� (���콺 Y ����)
            mouseY = Mathf.Clamp(mouseY, -30f, 60f); // ���Ʒ� ȸ�� ����
        }

        // ���콺 �ٷ� �� ��/�ƿ�
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom); // �� ���� ����
    }

    /// <summary>
    /// ī�޶��� ��ġ�� ����Ͽ� ��� �ֺ����� �̵�
    /// </summary>
    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0); // �����¿� ȸ�� ����
        Vector3 rotatedOffset = rotation * (Vector3.back * currentZoom + Vector3.up * 2f); // ȸ���� �� ������
        Vector3 targetPosition = target.position + rotatedOffset; // Ÿ�� ���� ��ġ ���

        // ī�޶� �̵�
        transform.position = targetPosition;
    }

    /// <summary>
    /// ī�޶� ��� �ٶ󺸵��� ȸ��
    /// </summary>
    private void UpdateCameraRotation()
    {
        // Ÿ�� ���� ���
        Vector3 direction = target.position - transform.position;

        if (direction != Vector3.zero)
        {
            // ��� ��� �������� ȸ��
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// ���ο� Ÿ�� ���� (��: �κ� ���� �� ȣ���)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// ī�޶� ȸ�� �� �� ���� �ʱ�ȭ
    /// </summary>
    public void ResetCamera()
    {
        mouseX = 0f;
        mouseY = 0f;
        currentZoom = 5f;
    }

    /// <summary>
    /// ī�޶� �ٶ󺸴� ������ ���� ���� ���� ��ȯ
    /// (�κ� �̵� � Ȱ�� ����)
    /// </summary>
    public Vector3 GetForwardDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f; // ���� ���⸸ ����
        return forward.normalized;
    }

    /// <summary>
    /// ī�޶� ���� ������ ������ ���� ���� ��ȯ
    /// </summary>
    public Vector3 GetRightDirection()
    {
        Vector3 right = transform.right;
        right.y = 0f; // ���� ���⸸ ����
        return right.normalized;
    }
}