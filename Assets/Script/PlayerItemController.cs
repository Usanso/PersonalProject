using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �κ��� ���� ����, (��ȭ �� ����� �Ϻ� ��������) ����ϴ� ��Ʈ�ѷ�
/// </summary>
public class PlayerItemController : MonoBehaviour
{
    // ������Ʈ ����
    private Rigidbody rb;
    private RobotActionRecorder actionRecorder;

    [Header("���� ���� ����")]
    [SerializeField] private Transform itemHoldPoint; // ������ �� ��ġ
    [SerializeField] private float pickupRange = 1.5f; // ������ �� �� �ִ� �Ÿ�
    private LayerMask itemLayerMask = 128; // ���� ���̾�

    [Header("�����")]
    [SerializeField] private bool showDebugGizmos = true;

    // ���� ����
    [SerializeField] private bool isPlayerControlled = false; // �÷��̾ ���� ������
    private GameObject carriedItem = null; // ���� ��� �ִ� ����

    #region �ʱ�ȭ
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // actionRecorder = GetComponent<RobotActionRecorder>();

        if (itemHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("ItemHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = Vector3.up * 0.5f; // ��� �ִ� ���� ��ġ (�κ� ����)
            itemHoldPoint = holdPoint.transform;
        }
    }
    #endregion

    #region Unity �����ֱ� �� �����
    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// ����� ����� �׸���
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // �Ⱦ� ����
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        // ������ Ȧ�� ����Ʈ
        if (itemHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemHoldPoint.position, 0.2f);
        }
    }
    #endregion

    #region �÷��̾� �Է� ó��

    /// <summary>
    /// �÷��̾� ���� ��� ���� (����)
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

        // �����̽��ٷ� ���� ���/����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleItemInteraction();
        }
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
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    private void DropItem()
    {
        if (carriedItem == null) return;

        // ���� ��ġ ��� (�κ� ����)
        Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 1f;

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

        // ��ȭ ���̶�� ��� �ൿ ���
        if (actionRecorder != null && actionRecorder.IsRecording())
        {
            actionRecorder.RecordSpecialAction("drop");
        }

        carriedItem = null;
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
}
