using UnityEngine;

/// <summary>
/// �������� Ÿ���� �����ϴ� ������
/// </summary>
public enum ItemType
{
    Box,        // ����
    Cylinder,   // ������ ����
    Sphere,     // ���� ����
    Special     // Ư�� ������
}

/// <summary>
/// �������� ���� ���¸� ��Ÿ���� ������
/// </summary>
public enum ItemCurrentState
{
    OnGround,        // �ٴڿ� ��������
    Held,            // �κ��� ��� ����
    PlacedCorrectly  // ��ǥ ��ġ�� ��Ȯ�� ��ġ��
}

/// <summary>
/// ����: ���� �������� �Ӽ��� ���� ����
/// ���� ����: ItemManager�� ���� ����, RobotController�� ���� ��ȣ�ۿ�
/// �ֿ� ���: ������ Ÿ��, ��ǥ ��ġ, ���� ���� ����
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class WarehouseItem : MonoBehaviour
{
    [Header("������ ����")]
    public int itemID = -1; // ItemManager���� �ڵ� �Ҵ�
    public ItemType itemType = ItemType.Box; // ������ Ÿ��
    public Vector3 targetPosition; // ��ǥ ��ġ ��ġ
    public float placementTolerance = 1.0f; // ��ǥ ��ġ ��� ���� ����

    [Header("������ ����")]
    public ItemCurrentState currentState = ItemCurrentState.OnGround; // ���� ����
    public int holdingRobotID = -1; // ���� ��� �ִ� �κ��� ID (-1�̸� �ƹ��� �� ��� ����)

    [Header("�ð��� ����")]
    public Material defaultMaterial; // �⺻ ��Ƽ����
    public Material targetMaterial; // ��ǥ ��ġ�� ���� �� ��Ƽ����
    public Material heldMaterial; // ������� �� ��Ƽ����

    [Header("�����")]
    public bool showTargetPosition = true; // ��ǥ ��ġ �ð�ȭ ����
    public Color targetPositionColor = Color.green; // ��ǥ ��ġ ����� ����

    private Renderer itemRenderer;
    private Rigidbody itemRigidbody;
    private Collider itemCollider;

    private void Awake()
    {
        itemRenderer = GetComponent<Renderer>();
        itemRigidbody = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        // �⺻ ����
        SetupDefaultPhysics();
    }

    private void Start()
    {
        // ItemManager�� �ڵ� ���
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.RegisterItem(this);
        }

        // ���̾�� �±� ����
        gameObject.layer = LayerMask.NameToLayer("Item");
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Item";
        }

        UpdateVisualState();
    }

    /// <summary>
    /// �⺻ ���� ����
    /// </summary>
    private void SetupDefaultPhysics()
    {
        if (itemRigidbody != null)
        {
            itemRigidbody.mass = 1f;
            itemRigidbody.drag = 1f;
            itemRigidbody.angularDrag = 5f;
        }

        if (itemCollider != null)
        {
            itemCollider.isTrigger = false;
        }
    }

    /// <summary>
    /// ������ ���¿� ���� �ð��� ������Ʈ
    /// </summary>
    private void UpdateVisualState()
    {
        if (itemRenderer == null) return;

        switch (currentState)
        {
            case ItemCurrentState.OnGround:
                if (defaultMaterial != null)
                    itemRenderer.material = defaultMaterial;
                break;

            case ItemCurrentState.Held:
                if (heldMaterial != null)
                    itemRenderer.material = heldMaterial;
                break;

            case ItemCurrentState.PlacedCorrectly:
                if (targetMaterial != null)
                    itemRenderer.material = targetMaterial;
                break;
        }
    }

    /// <summary>
    /// ������ ���¸� �����ϰ� �ð��� ������Ʈ
    /// </summary>
    /// <param name="newState">���ο� ����</param>
    public void SetState(ItemCurrentState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateVisualState();

            // ItemManager�� ���� ��ȭ �˸�
            if (ItemManager.Instance != null)
            {
                switch (newState)
                {
                    case ItemCurrentState.Held:
                        // �Ⱦ� �ÿ��� RobotController���� OnItemPickedUpByRobot ȣ��
                        break;
                    case ItemCurrentState.OnGround:
                        ItemManager.Instance.OnItemDroppedByRobot(this, transform.position);
                        break;
                    case ItemCurrentState.PlacedCorrectly:
                        // �ùٸ� ��ġ�� ItemManager���� �ڵ� ����
                        break;
                }
            }
        }
    }

    /// <summary>
    /// ��ǥ ��ġ������ �Ÿ� ��ȯ
    /// </summary>
    /// <returns>��ǥ ��ġ������ �Ÿ�</returns>
    public float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, targetPosition);
    }

    /// <summary>
    /// ��ǥ ��ġ�� �ùٸ��� ��ġ�Ǿ����� Ȯ��
    /// </summary>
    /// <returns>�ùٸ��� ��ġ�Ǿ����� true</returns>
    public bool IsPlacedCorrectly()
    {
        return GetDistanceToTarget() <= placementTolerance;
    }

    /// <summary>
    /// �������� ��ǥ ��ġ�� �����̵� (�����/�׽�Ʈ��)
    /// </summary>
    [ContextMenu("��ǥ ��ġ�� �̵�")]
    public void MoveToTargetPosition()
    {
        transform.position = targetPosition;
        SetState(ItemCurrentState.PlacedCorrectly);
    }

    /// <summary>
    /// ��ǥ ��ġ ����
    /// </summary>
    /// <param name="newTargetPosition">���ο� ��ǥ ��ġ</param>
    public void SetTargetPosition(Vector3 newTargetPosition)
    {
        targetPosition = newTargetPosition;
    }

    /// <summary>
    /// �������� �κ����� �Ⱦ��� �� ȣ��
    /// </summary>
    /// <param name="robot">�Ⱦ��ϴ� �κ�</param>
    public void OnPickedUp(RobotController robot)
    {
        holdingRobotID = robot.robotID;
        SetState(ItemCurrentState.Held);

        // ���� ���� ����
        if (itemRigidbody != null)
            itemRigidbody.isKinematic = true;
        if (itemCollider != null)
            itemCollider.isTrigger = true;

        // ItemManager�� �˸�
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemPickedUpByRobot(this, robot);
        }
    }

    /// <summary>
    /// �������� �κ��� ���� ��ӵ� �� ȣ��
    /// </summary>
    /// <param name="dropPosition">��� ��ġ</param>
    public void OnDropped(Vector3 dropPosition)
    {
        holdingRobotID = -1;

        // ���� ���� ����
        if (itemRigidbody != null)
            itemRigidbody.isKinematic = false;
        if (itemCollider != null)
            itemCollider.isTrigger = false;

        // ��ǥ ��ġ üũ
        if (IsPlacedCorrectly())
        {
            SetState(ItemCurrentState.PlacedCorrectly);
            gameObject.tag = "CompletedItem";
        }
        else
        {
            SetState(ItemCurrentState.OnGround);
            gameObject.tag = "Item";
        }

        // ItemManager�� �˸�
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemDroppedByRobot(this, dropPosition);
        }
    }

    /// <summary>
    /// �����Ϳ��� ��ǥ ��ġ�� ��� ���� �ð�ȭ
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showTargetPosition) return;

        // ��ǥ ��ġ ǥ��
        Gizmos.color = targetPositionColor;
        Gizmos.DrawWireSphere(targetPosition, placementTolerance);

        // ���� ��ġ���� ��ǥ ��ġ�� �� �׸���
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetPosition);

        // ��ǥ ��ġ�� ���� ť�� ǥ��
        Gizmos.color = targetPositionColor;
        Gizmos.DrawWireCube(targetPosition, Vector3.one * 0.5f);
    }

    /// <summary>
    /// �����Ϳ��� �׻� ���̴� ����� (��ǥ ��ġ��)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showTargetPosition) return;

        Gizmos.color = new Color(targetPositionColor.r, targetPositionColor.g, targetPositionColor.b, 0.3f);
        Gizmos.DrawSphere(targetPosition, placementTolerance);
    }
}