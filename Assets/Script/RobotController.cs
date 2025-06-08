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
    private bool isActive = false;
    private bool isPlaying = true;
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
        if (!isPlaying) return;
        RecordCurrentState();
        HandleInput();
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
            // ���� ����� ������ ã��
            GameObject closestItem = null;
            float closestDistance = float.MaxValue;

            foreach (Collider itemCollider in items)
            {
                // CompletedItem �±״� �̹� ������ �������̹Ƿ� ����
                if (itemCollider.CompareTag("CompletedItem")) continue;

                float distance = Vector3.Distance(transform.position, itemCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = itemCollider.gameObject;
                }
            }

            if (closestItem != null)
            {
                PickupItem(closestItem);
            }
        }
    }

    /// <summary>
    /// �������� ��� �κ��� ����
    /// </summary>
    /// <param name="item">�ݰ��� �ϴ� ������</param>
    private void PickupItem(GameObject item)
    {
        WarehouseItem warehouseItem = item.GetComponent<WarehouseItem>();

        if (warehouseItem == null)
        {
            Debug.LogWarning($"������ {item.name}�� WarehouseItem ������Ʈ�� �����ϴ�!");
            return;
        }

        // �̹� �ٸ� �κ��� ��� �ִ� �������� ���� ����
        if (warehouseItem.currentState == ItemCurrentState.Held)
        {
            return;
        }

        heldItem = item;

        // WarehouseItem�� OnPickedUp �޼��� ȣ�� (ItemManager ����)
        warehouseItem.OnPickedUp(this);

        // ������ ����
        item.transform.SetParent(itemHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// ��� �ִ� �������� ��������
    /// </summary>
    private void DropItem()
    {
        if (heldItem == null) return;

        Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        WarehouseItem warehouseItem = heldItem.GetComponent<WarehouseItem>();

        // ������ �и�
        heldItem.transform.SetParent(null);
        heldItem.transform.position = dropPosition;

        // WarehouseItem�� OnDropped �޼��� ȣ�� (ItemManager ����)
        if (warehouseItem != null)
        {
            warehouseItem.OnDropped(dropPosition);
        }
        else
        {
            // WarehouseItem ������Ʈ�� ���� ��� �⺻ ���� ����
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
        }

        heldItem = null;
    }

    /// <summary>
    /// ���� �κ� ���¸� ��� �����ڿ� ����
    /// </summary>
    private void RecordCurrentState()
    {
        if (RecordingManager.Instance == null || TimeManager.Instance == null) return;

        // ���� �κ� ���� ���
        RecordingManager.Instance.RecordRobotState(
            robotID,
            TimeManager.Instance.currentTime,
            transform.position,
            transform.rotation,
            heldItem != null
        );

        // ��� �ִ� �������� �ִٸ� �ش� �������� ���µ� ���
        if (heldItem != null)
        {
            WarehouseItem warehouseItem = heldItem.GetComponent<WarehouseItem>();
            if (warehouseItem != null && ItemManager.Instance != null)
            {
                // ItemManager�� �ڵ����� ������ ���¸� �����
                // ���� ȣ�� ���ʿ� (ItemManager.Update���� ó��)
            }
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
        this.isPlaying = isPlaying;
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
        if (RecordingManager.Instance == null || TimeManager.Instance == null) return;

        RobotState state = RecordingManager.Instance.GetRobotState(robotID, TimeManager.Instance.currentTime);
        if (state == null) return;

        // ��ġ�� ȸ�� ����
        transform.position = state.position;
        transform.rotation = state.rotation;

        // ������ ���� ������ ItemManager���� ó��
        // ���⼭�� �κ��� heldItem ������ ������Ʈ
        UpdateHeldItemReference();
    }

    /// <summary>
    /// �ð� ���� �� ��� �ִ� ������ ���� ������Ʈ
    /// </summary>
    public void UpdateHeldItemReference()
    {
        // ���� Ȧ�� ����Ʈ�� �������� �ִ��� Ȯ��
        if (itemHoldPoint.childCount > 0)
        {
            Transform childItem = itemHoldPoint.GetChild(0);
            heldItem = childItem.gameObject;
        }
        else
        {
            heldItem = null;
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
    /// <summary>
    /// ����� ��ȯ �޼���
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// ������ ������ ��ȯ �޼���
    /// </summary>
    public bool HasItem()
    {
        return heldItem != null;
    }

    /// <summary>
    /// �κ� ���� ����ÿ� �̵������� �ʱ�ȭ �Ͽ� �ߺ��� ������
    /// </summary>
    public void ResetMovementState()
    {
        smoothVelocity = Vector3.zero;
        // �ʿ��ϴٸ� inputDirection, ��Ÿ ���µ� �ʱ�ȭ
    }
}