using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 역할: 창고 내 모든 아이템의 생성, 배치, 상태 관리
/// 영향 관계: StageManager로부터 배치 정보 받음, RobotController와 픽업/드롭 상호작용
/// 주요 기능: 아이템 생성/제거, 목표 위치 체크, 정리 완료 검증
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    [Header("아이템 관리")]
    [SerializeField] private List<WarehouseItem> allItems = new List<WarehouseItem>(); // 게임 내 모든 아이템
    [SerializeField] private Transform itemContainer; // 아이템들을 담을 부모 오브젝트

    [Header("아이템 프리팹")]
    [SerializeField] private GameObject itemPrefab; // 기본 아이템 프리팹

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    // 아이템 상태 변화 이벤트
    public System.Action<WarehouseItem> OnItemPickedUp;
    public System.Action<WarehouseItem> OnItemDropped;
    public System.Action<WarehouseItem> OnItemPlacedCorrectly;

    private Dictionary<int, ItemState> itemStates = new Dictionary<int, ItemState>(); // 시간별 아이템 상태 기록

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

        // TimeManager 이벤트 구독
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
    /// 아이템 컨테이너 설정
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
    /// 씬에 이미 존재하는 아이템들을 초기화
    /// </summary>
    private void InitializeExistingItems()
    {
        // 씬에서 WarehouseItem 컴포넌트를 가진 모든 오브젝트 찾기
        WarehouseItem[] existingItems = FindObjectsOfType<WarehouseItem>();

        foreach (WarehouseItem item in existingItems)
        {
            RegisterItem(item);
        }

        Debug.Log($"ItemManager: {allItems.Count}개의 아이템을 등록했습니다.");
    }

    /// <summary>
    /// 새로운 아이템을 시스템에 등록
    /// </summary>
    /// <param name="item">등록할 아이템</param>
    public void RegisterItem(WarehouseItem item)
    {
        if (!allItems.Contains(item))
        {
            // 고유 ID 부여
            item.itemID = allItems.Count;
            allItems.Add(item);

            // 아이템을 컨테이너 하위로 이동
            item.transform.SetParent(itemContainer);

            // 초기 상태 기록
            RecordItemState(item);

            if (showDebugInfo)
            {
                Debug.Log($"아이템 등록: ID {item.itemID}, 타입: {item.itemType}");
            }
        }
    }

    /// <summary>
    /// 아이템을 시스템에서 제거
    /// </summary>
    /// <param name="item">제거할 아이템</param>
    public void UnregisterItem(WarehouseItem item)
    {
        if (allItems.Contains(item))
        {
            allItems.Remove(item);

            // 상태 기록에서도 제거
            itemStates.Remove(item.itemID);

            if (showDebugInfo)
            {
                Debug.Log($"아이템 제거: ID {item.itemID}");
            }
        }
    }

    /// <summary>
    /// 특정 위치에 새로운 아이템 생성
    /// </summary>
    /// <param name="position">생성 위치</param>
    /// <param name="itemType">아이템 타입</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <returns>생성된 아이템</returns>
    public WarehouseItem CreateItem(Vector3 position, ItemType itemType, Vector3 targetPosition)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("ItemManager: 아이템 프리팹이 설정되지 않았습니다!");
            return null;
        }

        GameObject newItemObj = Instantiate(itemPrefab, position, Quaternion.identity);
        WarehouseItem newItem = newItemObj.GetComponent<WarehouseItem>();

        if (newItem == null)
        {
            newItem = newItemObj.AddComponent<WarehouseItem>();
        }

        // 아이템 설정
        newItem.itemType = itemType;
        newItem.targetPosition = targetPosition;
        newItem.currentState = ItemCurrentState.OnGround;

        // 시스템에 등록
        RegisterItem(newItem);

        return newItem;
    }

    /// <summary>
    /// 로봇이 아이템을 집을 때 호출
    /// </summary>
    /// <param name="item">집은 아이템</param>
    /// <param name="robot">아이템을 집은 로봇</param>
    public void OnItemPickedUpByRobot(WarehouseItem item, RobotController robot)
    {
        if (item == null || robot == null) return;

        item.currentState = ItemCurrentState.Held;
        item.holdingRobotID = robot.robotID;

        RecordItemState(item);
        OnItemPickedUp?.Invoke(item);

        if (showDebugInfo)
        {
            Debug.Log($"아이템 픽업: ID {item.itemID}, 로봇 ID {robot.robotID}");
        }
    }

    /// <summary>
    /// 로봇이 아이템을 놓을 때 호출
    /// </summary>
    /// <param name="item">놓은 아이템</param>
    /// <param name="dropPosition">놓은 위치</param>
    public void OnItemDroppedByRobot(WarehouseItem item, Vector3 dropPosition)
    {
        if (item == null) return;

        item.currentState = ItemCurrentState.OnGround;
        item.holdingRobotID = -1;

        // 목표 위치에 정확히 놓였는지 확인
        CheckItemPlacement(item, dropPosition);

        RecordItemState(item);
        OnItemDropped?.Invoke(item);

        if (showDebugInfo)
        {
            Debug.Log($"아이템 드롭: ID {item.itemID}, 위치: {dropPosition}");
        }
    }

    /// <summary>
    /// 아이템이 목표 위치에 올바르게 배치되었는지 확인
    /// </summary>
    /// <param name="item">확인할 아이템</param>
    /// <param name="currentPosition">현재 위치</param>
    private void CheckItemPlacement(WarehouseItem item, Vector3 currentPosition)
    {
        float distance = Vector3.Distance(currentPosition, item.targetPosition);

        if (distance <= item.placementTolerance)
        {
            item.currentState = ItemCurrentState.PlacedCorrectly;
            item.gameObject.tag = "CompletedItem"; // 태그 변경으로 GameManager에서 승리 조건 체크

            OnItemPlacedCorrectly?.Invoke(item);

            if (showDebugInfo)
            {
                Debug.Log($"아이템 정리 완료: ID {item.itemID}");
            }
        }
    }

    /// <summary>
    /// 현재 시간에 아이템 상태를 기록
    /// </summary>
    /// <param name="item">기록할 아이템</param>
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

        // 딕셔너리 키: (아이템ID * 1000 + 시간)으로 고유하게 만듦
        int key = item.itemID * 10000 + Mathf.RoundToInt(currentTime * 10);
        itemStates[key] = state;
    }

    /// <summary>
    /// 특정 시간의 아이템 상태를 복원
    /// </summary>
    /// <param name="targetTime">복원할 시간</param>
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
            Debug.Log($"아이템 상태 복원 완료: 시간 {targetTime:F1}초");
        }
    }

    /// <summary>
    /// 특정 시간의 아이템 상태를 찾아서 반환
    /// </summary>
    /// <param name="itemID">아이템 ID</param>
    /// <param name="targetTime">찾을 시간</param>
    /// <returns>해당 시간의 아이템 상태</returns>
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
    /// 아이템에 상태를 적용
    /// </summary>
    /// <param name="item">적용할 아이템</param>
    /// <param name="state">적용할 상태</param>
    private void ApplyItemState(WarehouseItem item, ItemState state)
    {
        // 위치와 회전 복원
        item.transform.position = state.position;
        item.transform.rotation = state.rotation;

        // 아이템 상태 복원
        item.currentState = state.currentState;
        item.holdingRobotID = state.holdingRobotID;

        // 상태에 따른 물리 설정
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        Collider itemCollider = item.GetComponent<Collider>();

        switch (state.currentState)
        {
            case ItemCurrentState.Held:
                // 로봇이 들고 있는 상태 복원
                RestoreHeldState(item, state.holdingRobotID);
                break;

            case ItemCurrentState.OnGround:
                // 바닥에 놓인 상태 복원
                item.transform.SetParent(itemContainer);
                if (itemRb != null) itemRb.isKinematic = false;
                if (itemCollider != null) itemCollider.isTrigger = false;
                break;

            case ItemCurrentState.PlacedCorrectly:
                // 정리 완료 상태 복원
                item.transform.SetParent(itemContainer);
                item.gameObject.tag = "CompletedItem";
                if (itemRb != null) itemRb.isKinematic = false;
                if (itemCollider != null) itemCollider.isTrigger = false;
                break;
        }
    }

    /// <summary>
    /// 아이템이 로봇에게 들린 상태로 복원
    /// </summary>
    /// <param name="item">복원할 아이템</param>
    /// <param name="robotID">들고 있는 로봇 ID</param>
    private void RestoreHeldState(WarehouseItem item, int robotID)
    {
        // 해당 로봇 찾기
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
            // 아이템을 로봇의 홀드 포인트에 부착
            item.transform.SetParent(targetRobot.itemHoldPoint);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            // 물리 설정
            Rigidbody itemRb = item.GetComponent<Rigidbody>();
            if (itemRb != null) itemRb.isKinematic = true;

            Collider itemCollider = item.GetComponent<Collider>();
            if (itemCollider != null) itemCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 시간 변경 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="newTime">새로운 시간</param>
    private void OnTimeChanged(float newTime)
    {
        // 시간이 변경될 때마다 아이템 상태도 함께 복원
        // 단, 실시간 플레이 중이 아닐 때만
        if (!TimeManager.Instance.isPlaying)
        {
            RestoreItemStatesAtTime(newTime);
        }
    }

    /// <summary>
    /// 모든 아이템이 올바른 위치에 배치되었는지 확인
    /// </summary>
    /// <returns>모든 아이템이 정리되었으면 true</returns>
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
    /// 특정 타입의 아이템 개수 반환
    /// </summary>
    /// <param name="itemType">찾을 아이템 타입</param>
    /// <returns>해당 타입의 아이템 개수</returns>
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
    /// 디버그용: 모든 아이템 상태 출력
    /// </summary>
    [ContextMenu("디버그: 아이템 상태 출력")]
    public void DebugPrintItemStates()
    {
        Debug.Log("=== 아이템 상태 디버그 ===");
        foreach (WarehouseItem item in allItems)
        {
            Debug.Log($"ID: {item.itemID}, 타입: {item.itemType}, 상태: {item.currentState}, 로봇ID: {item.holdingRobotID}");
        }
    }
}