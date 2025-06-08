using UnityEngine;

/// <summary>
/// 아이템의 타입을 정의하는 열거형
/// </summary>
public enum ItemType
{
    Box,        // 상자
    Cylinder,   // 원통형 물건
    Sphere,     // 구형 물건
    Special     // 특수 아이템
}

/// <summary>
/// 아이템의 현재 상태를 나타내는 열거형
/// </summary>
public enum ItemCurrentState
{
    OnGround,        // 바닥에 놓여있음
    Held,            // 로봇이 들고 있음
    PlacedCorrectly  // 목표 위치에 정확히 배치됨
}

/// <summary>
/// 역할: 개별 아이템의 속성과 상태 관리
/// 영향 관계: ItemManager에 의해 관리, RobotController와 직접 상호작용
/// 주요 기능: 아이템 타입, 목표 위치, 현재 상태 저장
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class WarehouseItem : MonoBehaviour
{
    [Header("아이템 정보")]
    public int itemID = -1; // ItemManager에서 자동 할당
    public ItemType itemType = ItemType.Box; // 아이템 타입
    public Vector3 targetPosition; // 목표 배치 위치
    public float placementTolerance = 1.0f; // 목표 위치 허용 오차 범위

    [Header("아이템 상태")]
    public ItemCurrentState currentState = ItemCurrentState.OnGround; // 현재 상태
    public int holdingRobotID = -1; // 현재 들고 있는 로봇의 ID (-1이면 아무도 안 들고 있음)

    [Header("시각적 설정")]
    public Material defaultMaterial; // 기본 머티리얼
    public Material targetMaterial; // 목표 위치에 있을 때 머티리얼
    public Material heldMaterial; // 들려있을 때 머티리얼

    [Header("디버그")]
    public bool showTargetPosition = true; // 목표 위치 시각화 여부
    public Color targetPositionColor = Color.green; // 목표 위치 기즈모 색상

    private Renderer itemRenderer;
    private Rigidbody itemRigidbody;
    private Collider itemCollider;

    private void Awake()
    {
        itemRenderer = GetComponent<Renderer>();
        itemRigidbody = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        // 기본 설정
        SetupDefaultPhysics();
    }

    private void Start()
    {
        // ItemManager에 자동 등록
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.RegisterItem(this);
        }

        // 레이어와 태그 설정
        gameObject.layer = LayerMask.NameToLayer("Item");
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Item";
        }

        UpdateVisualState();
    }

    /// <summary>
    /// 기본 물리 설정
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
    /// 아이템 상태에 따른 시각적 업데이트
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
    /// 아이템 상태를 변경하고 시각적 업데이트
    /// </summary>
    /// <param name="newState">새로운 상태</param>
    public void SetState(ItemCurrentState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateVisualState();

            // ItemManager에 상태 변화 알림
            if (ItemManager.Instance != null)
            {
                switch (newState)
                {
                    case ItemCurrentState.Held:
                        // 픽업 시에는 RobotController에서 OnItemPickedUpByRobot 호출
                        break;
                    case ItemCurrentState.OnGround:
                        ItemManager.Instance.OnItemDroppedByRobot(this, transform.position);
                        break;
                    case ItemCurrentState.PlacedCorrectly:
                        // 올바른 배치는 ItemManager에서 자동 감지
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 목표 위치까지의 거리 반환
    /// </summary>
    /// <returns>목표 위치까지의 거리</returns>
    public float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, targetPosition);
    }

    /// <summary>
    /// 목표 위치에 올바르게 배치되었는지 확인
    /// </summary>
    /// <returns>올바르게 배치되었으면 true</returns>
    public bool IsPlacedCorrectly()
    {
        return GetDistanceToTarget() <= placementTolerance;
    }

    /// <summary>
    /// 아이템을 목표 위치로 순간이동 (디버그/테스트용)
    /// </summary>
    [ContextMenu("목표 위치로 이동")]
    public void MoveToTargetPosition()
    {
        transform.position = targetPosition;
        SetState(ItemCurrentState.PlacedCorrectly);
    }

    /// <summary>
    /// 목표 위치 설정
    /// </summary>
    /// <param name="newTargetPosition">새로운 목표 위치</param>
    public void SetTargetPosition(Vector3 newTargetPosition)
    {
        targetPosition = newTargetPosition;
    }

    /// <summary>
    /// 아이템이 로봇에게 픽업될 때 호출
    /// </summary>
    /// <param name="robot">픽업하는 로봇</param>
    public void OnPickedUp(RobotController robot)
    {
        holdingRobotID = robot.robotID;
        SetState(ItemCurrentState.Held);

        // 물리 설정 변경
        if (itemRigidbody != null)
            itemRigidbody.isKinematic = true;
        if (itemCollider != null)
            itemCollider.isTrigger = true;

        // ItemManager에 알림
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemPickedUpByRobot(this, robot);
        }
    }

    /// <summary>
    /// 아이템이 로봇에 의해 드롭될 때 호출
    /// </summary>
    /// <param name="dropPosition">드롭 위치</param>
    public void OnDropped(Vector3 dropPosition)
    {
        holdingRobotID = -1;

        // 물리 설정 복원
        if (itemRigidbody != null)
            itemRigidbody.isKinematic = false;
        if (itemCollider != null)
            itemCollider.isTrigger = false;

        // 목표 위치 체크
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

        // ItemManager에 알림
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemDroppedByRobot(this, dropPosition);
        }
    }

    /// <summary>
    /// 에디터에서 목표 위치와 허용 범위 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showTargetPosition) return;

        // 목표 위치 표시
        Gizmos.color = targetPositionColor;
        Gizmos.DrawWireSphere(targetPosition, placementTolerance);

        // 현재 위치에서 목표 위치로 선 그리기
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetPosition);

        // 목표 위치에 작은 큐브 표시
        Gizmos.color = targetPositionColor;
        Gizmos.DrawWireCube(targetPosition, Vector3.one * 0.5f);
    }

    /// <summary>
    /// 에디터에서 항상 보이는 기즈모 (목표 위치만)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showTargetPosition) return;

        Gizmos.color = new Color(targetPositionColor.r, targetPositionColor.g, targetPositionColor.b, 0.3f);
        Gizmos.DrawSphere(targetPosition, placementTolerance);
    }
}