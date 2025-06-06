using UnityEngine;

/// <summary>
/// �κ��� �̵�, ȸ��, ������ ��ȣ�ۿ�, �ð� ��� �� ����� �����ϴ� ��Ʈ�ѷ�
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    [Header("�κ� ����")]
    public int robotID; // ���� �κ� ID
    public float moveSpeed = 8f; // �̵� �ӵ�
    public float mouseSensitivity = 10f; // ���콺 ȸ�� �ΰ���
    public Transform itemHoldPoint; // �������� ��� ���� ��ġ
    public float pickupRange = 1.5f; // ������ ��ȣ�ۿ� �Ÿ�

    [Header("����")]
    public float acceleration = 1f;
    public float damping = 0.5f;

    private Rigidbody rb;
    [SerializeField] private bool isActive = false;
    private bool isGrounded = false;
    private GameObject heldItem = null;

    private Vector3 inputDirection;
    private Vector3 smoothVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetupItemHoldPoint();
    }

    private void Start()
    {
        TimeManager.Instance.OnTimeUpdated += OnTimeStateChanged;
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnTimeUpdated -= OnTimeStateChanged;
    }

    private void Update()
    {
        if (!isActive) return;

        HandleInput();
        RecordCurrentState();
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        HandleMovement();
        HandleRotation();
    }

    /// <summary>
    /// ����� �Է� ó�� (�̵� �� ��ȣ�ۿ�)
    /// </summary>
    private void HandleInput()
    {
        // �̵� �Է�
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // �̵� ���� ī�޶� �������� �ʱ�ȭ �� ��ֶ�����
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        inputDirection = (right * horizontal + forward * vertical).normalized;

        // ������ ��ȣ�ۿ�
        if (Input.GetMouseButtonDown(0))
        {
            HandleItemInteraction();
        }
    }

    /// <summary>
    /// �κ� �̵� ó�� (����/���� ����)
    /// </summary>
    private void HandleMovement()
    {
        // �Է� ���� x �̵��ӵ�
        Vector3 targetVelocity = inputDirection * moveSpeed;

        // ������
        smoothVelocity = Vector3.Lerp(smoothVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (inputDirection.magnitude < 0.1f)
        {
            smoothVelocity = Vector3.Lerp(smoothVelocity, Vector3.zero, damping * Time.fixedDeltaTime);
        }

        // ���� ������� �̵� ����
        if (isGrounded)
        {
            Vector3 velocityChange = smoothVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// ���콺 �Է¿� ���� �κ� ȸ�� ó��
    /// </summary>
    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);
    }

    /// <summary>
    /// ������ �ݱ� �Ǵ� ��������, ��۷� ó��
    /// </summary>
    private void HandleItemInteraction()
    {
        if (heldItem == null)
        {
            TryPickupItem();
        }
        else
        {
            DropItem();
        }
    }

    /// <summary>
    /// ��ó �������� �ִٸ� �ݱ� �õ�
    /// </summary>
    private void TryPickupItem()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, pickupRange, LayerMask.GetMask("Item"));

        if (items.Length > 0)
        {
            GameObject closestItem = items[0].gameObject;
            PickupItem(closestItem);
        }
    }

    /// <summary>
    /// �������� ��� �κ��� ����
    /// </summary>
    /// <param name="item">�ݰ��� �ϴ� ������</param>
    private void PickupItem(GameObject item)
    {
        heldItem = item;
        item.transform.SetParent(itemHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = true;
        }

        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// ��� �ִ� �������� ��������
    /// </summary>
    private void DropItem()
    {
        if (heldItem == null) return;

        Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        heldItem.transform.SetParent(null);
        heldItem.transform.position = dropPosition;

        Rigidbody itemRb = heldItem.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = false;
        }

        Collider itemCollider = heldItem.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = false;
        }

        heldItem = null;
    }

    /// <summary>
    /// ���� �κ� ���¸� ��� �����ڿ� ����
    /// </summary>
    private void RecordCurrentState()
    {
        if (RecordingManager.Instance != null && TimeManager.Instance != null)
        {
            RecordingManager.Instance.RecordRobotState(
                robotID,
                TimeManager.Instance.currentTime,
                transform.position,
                transform.rotation,
                heldItem != null
            );
        }
    }

    /// <summary>
    /// ������ Ȧ�� ����Ʈ�� ���ٸ� �ڵ� ����
    /// </summary>
    private void SetupItemHoldPoint()
    {
        if (itemHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("ItemHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = Vector3.up * 1.5f;
            itemHoldPoint = holdPoint.transform;
        }
    }

    /// <summary>
    /// �ð� ���� �� ��ϵ� ���·� �ǵ���
    /// </summary>
    /// <param name="isPlaying">�ð� ��� ����</param>
    private void OnTimeStateChanged(bool isPlaying)
    {
        if (!isPlaying)
        {
            ApplyRecordedState();
        }
    }

    /// <summary>
    /// ��ϵ� �κ� ���¸� ����
    /// </summary>
    private void ApplyRecordedState()
    {
        if (RecordingManager.Instance != null && TimeManager.Instance != null)
        {
            RobotState state = RecordingManager.Instance.GetRobotState(robotID, TimeManager.Instance.currentTime);
            if (state != null)
            {
                transform.position = state.position;
                transform.rotation = state.rotation;

                // Handle item state
                if (state.hasItem && heldItem == null)
                {
                    // Find and pickup nearest item
                    TryPickupItem();
                }
                else if (!state.hasItem && heldItem != null)
                {
                    DropItem();
                }
            }
        }
    }

    /// <summary>
    /// �κ� �Է� Ȱ��ȭ ����
    /// </summary>
    /// <param name="active">Ȱ��ȭ ����</param>
    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>
    /// ���� �浹 �� isGrounded Ȱ��ȭ
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    /// <summary>
    /// ���� ��Ż �� isGrounded ��Ȱ��ȭ
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// �����Ϳ��� �Ⱦ� ���� �� ������ ��ġ �ð�ȭ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        if (itemHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemHoldPoint.position, 0.2f);
        }
    }
}