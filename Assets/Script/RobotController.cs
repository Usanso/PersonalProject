using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �κ��� �̵�, ���� ����, �浹 ó���� ����ϴ� ��Ʈ�ѷ�
/// </summary>
public class RobotController : MonoBehaviour
{
    // ������Ʈ ����
    private Rigidbody rb;
    private RobotActionRecorder actionRecorder;

    // �浹 ������
    private Collider robotCollider;

    [Header("�̵� ����")]
    [SerializeField] private float moveSpeed = 8f;

    [SerializeField] private float deceleration = 15f; // ���ӵ�
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float rotationAcceleration = 720f; // ȸ�� ���ӵ�

    [Header("���� ���� ����")]
    [SerializeField] private Transform itemHoldPoint; // ������ �� ��ġ
    [SerializeField] private float pickupRange = 1.5f; // ������ �� �� �ִ� �Ÿ�
    [SerializeField] private LayerMask itemLayerMask = 1; // ���� ���̾�

    [Header("�浹 ����")]
    [SerializeField] private float robotRadius = 0.5f; // �κ� �浹 ������
    [SerializeField] private LayerMask obstacleLayerMask = 1; // ��ֹ� ���̾�

    [Header("�����")]
    [SerializeField] private bool showDebugGizmos = true;

    // ���� ����
    [SerializeField] private bool isPlayerControlled = false; // �÷��̾ ���� ������
    private GameObject carriedItem = null; // ���� ��� �ִ� ����
    private Vector3 moveInput = Vector3.zero; // �Է� ����
    private Vector3 currentVelocity = Vector3.zero; // ���� �ӵ�
    private float currentRotationVelocity = 0f; // ���� ȸ�� �ӵ�

    #region �ʱ�ȭ

    /// <summary>
    /// ������Ʈ �ʱ�ȭ
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // actionRecorder = GetComponent<RobotActionRecorder>();
        robotCollider = GetComponent<Collider>();

        //// Rigidbody ����
        //if (rb != null)
        //{
        //    rb.freezeRotation = true; // ������ ȸ�� ���� (��ũ��Ʈ�� ����)
        //    rb.drag = 8f; // ���� ������ �̲����� ����
        //    rb.angularDrag = 10f; // ȸ�� ����
        //    rb.mass = 2f; // ���� ������ ������ ���

        //    // �����߽��� ���缭 �Ѿ����� �ʰ� ����
        //    rb.centerOfMass = new Vector3(0, -0.5f, 0);
        //}

        // ������ Ȧ�� ����Ʈ�� ���ٸ� �ڵ� ����
        if (itemHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("ItemHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = Vector3.up * 1.5f; // �κ� ����
            itemHoldPoint = holdPoint.transform;
        }
    }

    #endregion

    #region �÷��̾� �Է� ó��

    /// <summary>
    /// �÷��̾� ���� ��� ����
    /// </summary>
    /// <param name="controlled">�÷��̾ �������� ����</param>
    public void SetPlayerControlled(bool controlled)
    {
        isPlayerControlled = controlled;

        if (controlled && actionRecorder != null)
        {
            // �÷��̾� ���� ���� �� ��ȭ ����
            actionRecorder.StartRecording();
        }
        else if (!controlled && actionRecorder != null)
        {
            // �÷��̾� ���� ���� �� ��ȭ ����
            actionRecorder.StopRecording();
        }
    }

    /// <summary>
    /// �Է� ó�� (Update���� ȣ��)
    /// </summary>
    private void HandleInput()
    {
        if (!isPlayerControlled) return;

        // WASD �Ǵ� ȭ��ǥ Ű�� �̵�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveInput = new Vector3(horizontal, 0, vertical).normalized;

        // �����̽��ٷ� ���� ���/����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleItemInteraction();
        }
    }

    #endregion

    #region �̵� ó��

    /// <summary>
    /// �κ� �̵� ó�� (�ﰢ �����ϴ� ����/���� ���)
    /// </summary>
    private void HandleMovement()
    {
        // �̵� ó�� (�ӵ� ���)
        rb.velocity = moveInput * moveSpeed + new Vector3(0, rb.velocity.y, 0);

        // �̵� ���� (�浹 �˻� ����)
        if (currentVelocity.magnitude > 0.01f)
        {
            Vector3 targetPosition = transform.position + currentVelocity * Time.fixedDeltaTime;

            if (CanMoveTo(targetPosition))
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                // �浹 �� �ش� ���� �ӵ� ����
                Vector3 moveDirection = (targetPosition - transform.position).normalized;
                currentVelocity = Vector3.ProjectOnPlane(currentVelocity, moveDirection);
            }
        }
    }

    /// <summary>
    /// Ư�� ��ġ�� �̵� �������� �˻�
    /// </summary>
    /// <param name="targetPosition">��ǥ ��ġ</param>
    /// <returns>�̵� ���� ����</returns>
    private bool CanMoveTo(Vector3 targetPosition)
    {
        // ��ü ���·� �浹 �˻�
        Collider[] obstacles = Physics.OverlapSphere(targetPosition, robotRadius, obstacleLayerMask);

        // �ڱ� �ڽ��� ����
        foreach (var obstacle in obstacles)
        {
            if (obstacle != robotCollider)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ���α׷��� ������� Ư�� ��ġ�� �̵� (��� �� ���)
    /// </summary>
    /// <param name="targetPosition">��ǥ ��ġ</param>
    public void MoveToPosition(Vector3 targetPosition)
    {
        // ��� �ÿ��� ���� ��Ģ �����ϰ� ���� �̵�
        transform.position = targetPosition;

        // ��� �߿��� �ӵ� �ʱ�ȭ
        currentVelocity = Vector3.zero;
        currentRotationVelocity = 0f;
    }

    #endregion

    #region ���� ����

    /// <summary>
    /// ���� ��ȣ�ۿ� ó�� (���/����)
    /// </summary>
    private void HandleItemInteraction()
    {
        if (carriedItem == null)
        {
            // ���� ��� �õ�
            TryPickupItem();
        }
        else
        {
            // ���� ����
            DropItem();
        }
    }

    /// <summary>
    /// �ֺ� ���� ��� �õ�
    /// </summary>
    private void TryPickupItem()
    {
        // �ֺ� ���� �˻�
        Collider[] nearbyItems = Physics.OverlapSphere(transform.position, pickupRange, itemLayerMask);

        GameObject closestItem = null;
        float closestDistance = float.MaxValue;

        // ���� ����� ���� ã��
        foreach (var itemCollider in nearbyItems)
        {
            float distance = Vector3.Distance(transform.position, itemCollider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = itemCollider.gameObject;
            }
        }

        // ���� ��� ����
        if (closestItem != null)
        {
            PickupItem(closestItem);
        }
    }

    /// <summary>
    /// ���� ��� ����
    /// </summary>
    /// <param name="item">�� ����</param>
    private void PickupItem(GameObject item)
    {
        carriedItem = item;

        // ������ Ȧ�� ����Ʈ�� �̵�
        item.transform.SetParent(itemHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // ���� ��Ȱ��ȭ
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = true;
        }

        // �浹 ��Ȱ��ȭ
        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }

        // ��ȭ ���̶�� �Ⱦ� �ൿ ���
        if (actionRecorder != null && actionRecorder.IsRecording())
        {
            actionRecorder.RecordSpecialAction("pickup");
        }

        Debug.Log($"���� �Ⱦ�: {item.name}");
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    private void DropItem()
    {
        if (carriedItem == null) return;

        // ���� ��ġ ��� (�κ� ����)
        Vector3 dropPosition = transform.position + transform.forward * 1.5f;

        // ������ ����� �̵�
        carriedItem.transform.SetParent(null);
        carriedItem.transform.position = dropPosition;

        // ���� Ȱ��ȭ
        Rigidbody itemRb = carriedItem.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = false;
        }

        // �浹 Ȱ��ȭ
        Collider itemCollider = carriedItem.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = false;
        }

        Debug.Log($"���� ���: {carriedItem.name}");

        // ��ȭ ���̶�� ��� �ൿ ���
        if (actionRecorder != null && actionRecorder.IsRecording())
        {
            actionRecorder.RecordSpecialAction("drop");
        }

        carriedItem = null;
    }

    /// <summary>
    /// ��� �� �Ⱦ� �ൿ ����
    /// </summary>
    public void ExecutePickupAction()
    {
        if (carriedItem == null)
        {
            TryPickupItem();
        }
    }

    /// <summary>
    /// ��� �� ��� �ൿ ����
    /// </summary>
    public void ExecuteDropAction()
    {
        if (carriedItem != null)
        {
            DropItem();
        }
    }

    #endregion

    #region ���� Ȯ�� �޼���

    /// <summary>
    /// ������ ��� �ִ��� Ȯ��
    /// </summary>
    /// <returns>���� ���� ����</returns>
    public bool IsCarryingItem()
    {
        return carriedItem != null;
    }

    /// <summary>
    /// ��� �ִ� ������ ��ġ ��ȯ
    /// </summary>
    /// <returns>���� ��ġ</returns>
    public Vector3 GetCarriedItemPosition()
    {
        if (carriedItem != null)
            return carriedItem.transform.position;
        return Vector3.zero;
    }

    /// <summary>
    /// ��� �ִ� ������ ��ġ ���� (��� �� ���)
    /// </summary>
    /// <param name="position">������ ��ġ</param>
    public void SetCarriedItemPosition(Vector3 position)
    {
        if (carriedItem != null)
        {
            carriedItem.transform.position = position;
        }
    }

    /// <summary>
    /// �÷��̾� ���� ������ Ȯ��
    /// </summary>
    /// <returns>�÷��̾� ���� ����</returns>
    public bool IsPlayerControlled()
    {
        return isPlayerControlled;
    }

    #endregion

    #region �浹 ����

    /// <summary>
    /// �ٸ� �κ��� �浹 �� ȣ��
    /// </summary>
    /// <param name="other">�浹�� ��ü</param>
    private void OnTriggerEnter(Collider other)
    {
        // �ٸ� �κ��� �浹�� ���
        RobotController otherRobot = other.GetComponent<RobotController>();
        if (otherRobot != null)
        {
            Debug.LogWarning($"{gameObject.name}�� {other.gameObject.name}�� �浹!");

            // ���� �Ŵ����� �浹 �˸� (���� ����)
            // GameManager.Instance?.OnRobotCollision(this, otherRobot);
        }
    }

    #endregion

    #region Unity �����ֱ�

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// ����� ����� �׸���
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // �κ� �浹 ����
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, robotRadius);

        // �Ⱦ� ����
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        // �̵� ���� (���� �ӵ� ����)
        if (currentVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, currentVelocity.normalized * 2f);
        }

        // ������ Ȧ�� ����Ʈ
        if (itemHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemHoldPoint.position, 0.2f);
        }
    }

    #endregion
}
