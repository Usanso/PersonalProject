using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����: â�� �� ��� �������� ����, ��ġ, ���� ����
/// ���� ����: StageManager�κ��� ��ġ ���� ����, RobotController�� �Ⱦ�/��� ��ȣ�ۿ�
/// �ֿ� ���: ������ ����/����, ��ǥ ��ġ üũ, ���� �Ϸ� ����
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("������ ����")]
    [SerializeField] private List<WarehouseItem> allItems = new List<WarehouseItem>(); // ���� �� ��� ������
    [SerializeField] private Transform itemContainer; // �����۵��� ���� �θ� ������Ʈ

    [Header("������ ������")]
    [SerializeField] private GameObject itemPrefab; // �⺻ ������ ������

    [Header("�����")]
    [SerializeField] private bool showDebugInfo = true;

    // ������ ���� ��ȭ �̺�Ʈ
    public System.Action<WarehouseItem> OnItemPickedUp;
    public System.Action<WarehouseItem> OnItemDropped;
    public System.Action<WarehouseItem> OnItemPlacedCorrectly;

    private Dictionary<int, ItemState> itemStates = new Dictionary<int, ItemState>(); // �ð��� ������ ���� ���

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetupItemContainer();
    }

    private void Start()
    {
        InitializeExistingItems();

        // TimeManager �̺�Ʈ ����
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += OnTimeChanged;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= OnTimeChanged;
        }
    }

    /// <summary>
    /// ������ �����̳� ����
    /// </summary>
    private void SetupItemContainer()
    {
        if (itemContainer == null)
        {
            GameObject container = new GameObject("ItemContainer");
            container.transform.SetParent(transform);
            itemContainer = container.transform;
        }
    }

    /// <summary>
    /// ���� �̹� �����ϴ� �����۵��� �ʱ�ȭ
    /// </summary>
    private void InitializeExistingItems()
    {
        // ������ WarehouseItem ������Ʈ�� ���� ��� ������Ʈ ã��
        WarehouseItem[] existingItems = FindObjectsOfType<WarehouseItem>();

        foreach (WarehouseItem item in existingItems)
        {
            RegisterItem(item);
        }

        Debug.Log($"ItemManager: {allItems.Count}���� �������� ����߽��ϴ�.");
    }

    /// <summary>
    /// ���ο� �������� �ý��ۿ� ���
    /// </summary>
    /// <param name="item">����� ������</param>
    public void RegisterItem(WarehouseItem item)
    {
        if (!allItems.Contains(item))
        {
            // ���� ID �ο�
            item.itemID = allItems.Count;
            allItems.Add(item);

            // �������� �����̳� ������ �̵�
            item.transform.SetParent(itemContainer);

            // �ʱ� ���� ���
            RecordItemState(item);

            if (showDebugInfo)
            {
                Debug.Log($"������ ���: ID {item.itemID}, Ÿ��: {item.itemType}");
            }
        }
    }

    /// <summary>
    /// �������� �ý��ۿ��� ����
    /// </summary>
    /// <param name="item">������ ������</param>
    public void UnregisterItem(WarehouseItem item)
    {
        if (allItems.Contains(item))
        {
            allItems.Remove(item);

            // ���� ��Ͽ����� ����
            itemStates.Remove(item.itemID);

            if (showDebugInfo)
            {
                Debug.Log($"������ ����: ID {item.itemID}");
            }
        }
    }

    /// <summary>
    /// Ư�� ��ġ�� ���ο� ������ ����
    /// </summary>
    /// <param name="position">���� ��ġ</param>
    /// <param name="itemType">������ Ÿ��</param>
    /// <param name="targetPosition">��ǥ ��ġ</param>
    /// <returns>������ ������</returns>
    public WarehouseItem CreateItem(Vector3 position, ItemType itemType, Vector3 targetPosition)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("ItemManager: ������ �������� �������� �ʾҽ��ϴ�!");
            return null;
        }

        GameObject newItemObj = Instantiate(itemPrefab, position, Quaternion.identity);
        WarehouseItem newItem = newItemObj.GetComponent<WarehouseItem>();

        if (newItem == null)
        {
            newItem = newItemObj.AddComponent<WarehouseItem>();
        }

        // ������ ����
        newItem.itemType = itemType;
        newItem.targetPosition = targetPosition;
        newItem.currentState = ItemCurrentState.OnGround;

        // �ý��ۿ� ���
        RegisterItem(newItem);

        return newItem;
    }

    /// <summary>
    /// �κ��� �������� ���� �� ȣ��
    /// </summary>
    /// <param name="item">���� ������</param>
    /// <param name="robot">�������� ���� �κ�</param>
    public void OnItemPickedUpByRobot(WarehouseItem item, RobotController robot)
    {
        if (item == null || robot == null) return;

        item.currentState = ItemCurrentState.Held;
        item.holdingRobotID = robot.robotID;

        RecordItemState(item);
        OnItemPickedUp?.Invoke(item);

        if (showDebugInfo)
        {
            Debug.Log($"������ �Ⱦ�: ID {item.itemID}, �κ� ID {robot.robotID}");
        }
    }

    /// <summary>
    /// �κ��� �������� ���� �� ȣ��
    /// </summary>
    /// <param name="item">���� ������</param>
    /// <param name="dropPosition">���� ��ġ</param>
    public void OnItemDroppedByRobot(WarehouseItem item, Vector3 dropPosition)
    {
        if (item == null) return;

        item.currentState = ItemCurrentState.OnGround;
        item.holdingRobotID = -1;

        // ��ǥ ��ġ�� ��Ȯ�� �������� Ȯ��
        CheckItemPlacement(item, dropPosition);

        RecordItemState(item);
        OnItemDropped?.Invoke(item);

        if (showDebugInfo)
        {
            Debug.Log($"������ ���: ID {item.itemID}, ��ġ: {dropPosition}");
        }
    }

    /// <summary>
    /// �������� ��ǥ ��ġ�� �ùٸ��� ��ġ�Ǿ����� Ȯ��
    /// </summary>
    /// <param name="item">Ȯ���� ������</param>
    /// <param name="currentPosition">���� ��ġ</param>
    private void CheckItemPlacement(WarehouseItem item, Vector3 currentPosition)
    {
        float distance = Vector3.Distance(currentPosition, item.targetPosition);

        if (distance <= item.placementTolerance)
        {
            item.currentState = ItemCurrentState.PlacedCorrectly;
            item.gameObject.tag = "CompletedItem"; // �±� �������� GameManager���� �¸� ���� üũ

            OnItemPlacedCorrectly?.Invoke(item);

            if (showDebugInfo)
            {
                Debug.Log($"������ ���� �Ϸ�: ID {item.itemID}");
            }
        }
    }

    /// <summary>
    /// ���� �ð��� ������ ���¸� ���
    /// </summary>
    /// <param name="item">����� ������</param>
    private void RecordItemState(WarehouseItem item)
    {
        if (TimeManager.Instance == null) return;

        float currentTime = TimeManager.Instance.currentTime;

        ItemState state = new ItemState(
            item.transform.position,
            item.transform.rotation,
            item.currentState,
            item.holdingRobotID
        );

        // ��ųʸ� Ű: (������ID * 1000 + �ð�)���� �����ϰ� ����
        int key = item.itemID * 10000 + Mathf.RoundToInt(currentTime * 10);
        itemStates[key] = state;
    }

    /// <summary>
    /// Ư�� �ð��� ������ ���¸� ����
    /// </summary>
    /// <param name="targetTime">������ �ð�</param>
    public void RestoreItemStatesAtTime(float targetTime)
    {
        foreach (WarehouseItem item in allItems)
        {
            if (item == null) continue;

            ItemState restoredState = GetItemStateAtTime(item.itemID, targetTime);

            if (restoredState != null)
            {
                ApplyItemState(item, restoredState);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"������ ���� ���� �Ϸ�: �ð� {targetTime:F1}��");
        }
    }

    /// <summary>
    /// Ư�� �ð��� ������ ���¸� ã�Ƽ� ��ȯ
    /// </summary>
    /// <param name="itemID">������ ID</param>
    /// <param name="targetTime">ã�� �ð�</param>
    /// <returns>�ش� �ð��� ������ ����</returns>
    private ItemState GetItemStateAtTime(int itemID, float targetTime)
    {
        ItemState closestState = null;
        float closestTimeDiff = float.MaxValue;

        foreach (var kvp in itemStates)
        {
            int stateItemID = kvp.Key / 10000;
            float stateTime = (kvp.Key % 10000) / 10f;

            if (stateItemID == itemID && stateTime <= targetTime)
            {
                float timeDiff = targetTime - stateTime;
                if (timeDiff < closestTimeDiff)
                {
                    closestTimeDiff = timeDiff;
                    closestState = kvp.Value;
                }
            }
        }

        return closestState;
    }

    /// <summary>
    /// �����ۿ� ���¸� ����
    /// </summary>
    /// <param name="item">������ ������</param>
    /// <param name="state">������ ����</param>
    private void ApplyItemState(WarehouseItem item, ItemState state)
    {
        // ��ġ�� ȸ�� ����
        item.transform.position = state.position;
        item.transform.rotation = state.rotation;

        // ������ ���� ����
        item.currentState = state.currentState;
        item.holdingRobotID = state.holdingRobotID;

        // ���¿� ���� ���� ����
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        Collider itemCollider = item.GetComponent<Collider>();

        switch (state.currentState)
        {
            case ItemCurrentState.Held:
                // �κ��� ��� �ִ� ���� ����
                RestoreHeldState(item, state.holdingRobotID);
                break;

            case ItemCurrentState.OnGround:
                // �ٴڿ� ���� ���� ����
                item.transform.SetParent(itemContainer);
                if (itemRb != null) itemRb.isKinematic = false;
                if (itemCollider != null) itemCollider.isTrigger = false;
                break;

            case ItemCurrentState.PlacedCorrectly:
                // ���� �Ϸ� ���� ����
                item.transform.SetParent(itemContainer);
                item.gameObject.tag = "CompletedItem";
                if (itemRb != null) itemRb.isKinematic = false;
                if (itemCollider != null) itemCollider.isTrigger = false;
                break;
        }
    }

    /// <summary>
    /// �������� �κ����� �鸰 ���·� ����
    /// </summary>
    /// <param name="item">������ ������</param>
    /// <param name="robotID">��� �ִ� �κ� ID</param>
    private void RestoreHeldState(WarehouseItem item, int robotID)
    {
        // �ش� �κ� ã��
        RobotController targetRobot = null;
        foreach (RobotController robot in FindObjectsOfType<RobotController>())
        {
            if (robot.robotID == robotID)
            {
                targetRobot = robot;
                break;
            }
        }

        if (targetRobot != null && targetRobot.itemHoldPoint != null)
        {
            // �������� �κ��� Ȧ�� ����Ʈ�� ����
            item.transform.SetParent(targetRobot.itemHoldPoint);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            // ���� ����
            Rigidbody itemRb = item.GetComponent<Rigidbody>();
            if (itemRb != null) itemRb.isKinematic = true;

            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider != null) itemCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// �ð� ���� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    /// </summary>
    /// <param name="newTime">���ο� �ð�</param>
    private void OnTimeChanged(float newTime)
    {
        // �ð��� ����� ������ ������ ���µ� �Բ� ����
        // ��, �ǽð� �÷��� ���� �ƴ� ����
        if (!TimeManager.Instance.isPlaying)
        {
            RestoreItemStatesAtTime(newTime);
        }
    }

    /// <summary>
    /// ��� �������� �ùٸ� ��ġ�� ��ġ�Ǿ����� Ȯ��
    /// </summary>
    /// <returns>��� �������� �����Ǿ����� true</returns>
    public bool AreAllItemsPlacedCorrectly()
    {
        foreach (WarehouseItem item in allItems)
        {
            if (item.currentState != ItemCurrentState.PlacedCorrectly)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Ư�� Ÿ���� ������ ���� ��ȯ
    /// </summary>
    /// <param name="itemType">ã�� ������ Ÿ��</param>
    /// <returns>�ش� Ÿ���� ������ ����</returns>
    public int GetItemCountByType(ItemType itemType)
    {
        int count = 0;
        foreach (WarehouseItem item in allItems)
        {
            if (item.itemType == itemType)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// ����׿�: ��� ������ ���� ���
    /// </summary>
    [ContextMenu("�����: ������ ���� ���")]
    public void DebugPrintItemStates()
    {
        Debug.Log("=== ������ ���� ����� ===");
        foreach (WarehouseItem item in allItems)
        {
            Debug.Log($"ID: {item.itemID}, Ÿ��: {item.itemType}, ����: {item.currentState}, �κ�ID: {item.holdingRobotID}");
        }
    }
}