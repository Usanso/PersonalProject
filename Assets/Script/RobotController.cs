using UnityEngine;

/// <summary>
/// 로봇의 이동, 회전, 아이템 상호작용, 시간 기록 및 재생을 관리하는 컨트롤러
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    [Header("로봇 세팅")]
    public int robotID; // 고유 로봇 ID
    public float moveSpeed = 8f; // 이동 속도
    public float mouseSensitivity = 10f; // 마우스 회전 민감도
    public Transform itemHoldPoint; // 아이템을 들고 있을 위치
    public float pickupRange = 1.5f; // 아이템 상호작용 거리

    [Header("물리")]
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
    /// 사용자 입력 처리 (이동 및 상호작용)
    /// </summary>
    private void HandleInput()
    {
        // 이동 입력
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 이동 방향 카메라 기준으로 초기화 및 노멀라이즈
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        inputDirection = (right * horizontal + forward * vertical).normalized;

        // 아이템 상호작용
        if (Input.GetMouseButtonDown(0))
        {
            HandleItemInteraction();
        }
    }

    /// <summary>
    /// 로봇 이동 처리 (가속/감속 포함)
    /// </summary>
    private void HandleMovement()
    {
        // 입력 방향 x 이동속도
        Vector3 targetVelocity = inputDirection * moveSpeed;

        // 스무스
        smoothVelocity = Vector3.Lerp(smoothVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        if (inputDirection.magnitude < 0.1f)
        {
            smoothVelocity = Vector3.Lerp(smoothVelocity, Vector3.zero, damping * Time.fixedDeltaTime);
        }

        // 땅에 닿았을때 이동 가능
        if (isGrounded)
        {
            Vector3 velocityChange = smoothVelocity - new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// 마우스 입력에 따라 로봇 회전 처리
    /// </summary>
    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);
    }

    /// <summary>
    /// 아이템 줍기 또는 내려놓기, 토글로 처리
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
    /// 근처 아이템이 있다면 줍기 시도
    /// </summary>
    private void TryPickupItem()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, pickupRange, LayerMask.GetMask("Item"));

        if (items.Length > 0)
        {
            // 가장 가까운 아이템 찾기
            GameObject closestItem = null;
            float closestDistance = float.MaxValue;

            foreach (Collider itemCollider in items)
            {
                // CompletedItem 태그는 이미 정리된 아이템이므로 제외
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
    /// 아이템을 들고 로봇에 부착
    /// </summary>
    /// <param name="item">줍고자 하는 아이템</param>
    private void PickupItem(GameObject item)
    {
        WarehouseItem warehouseItem = item.GetComponent<WarehouseItem>();

        if (warehouseItem == null)
        {
            Debug.LogWarning($"아이템 {item.name}에 WarehouseItem 컴포넌트가 없습니다!");
            return;
        }

        // 이미 다른 로봇이 들고 있는 아이템은 줍지 않음
        if (warehouseItem.currentState == ItemCurrentState.Held)
        {
            return;
        }

        heldItem = item;

        // WarehouseItem의 OnPickedUp 메서드 호출 (ItemManager 연동)
        warehouseItem.OnPickedUp(this);

        // 물리적 부착
        item.transform.SetParent(itemHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 들고 있는 아이템을 내려놓음
    /// </summary>
    private void DropItem()
    {
        if (heldItem == null) return;

        Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.5f;

        WarehouseItem warehouseItem = heldItem.GetComponent<WarehouseItem>();

        // 물리적 분리
        heldItem.transform.SetParent(null);
        heldItem.transform.position = dropPosition;

        // WarehouseItem의 OnDropped 메서드 호출 (ItemManager 연동)
        if (warehouseItem != null)
        {
            warehouseItem.OnDropped(dropPosition);
        }
        else
        {
            // WarehouseItem 컴포넌트가 없는 경우 기본 물리 설정
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
    /// 현재 로봇 상태를 기록 관리자에 전달
    /// </summary>
    private void RecordCurrentState()
    {
        if (RecordingManager.Instance == null || TimeManager.Instance == null) return;

        // 기존 로봇 상태 기록
        RecordingManager.Instance.RecordRobotState(
            robotID,
            TimeManager.Instance.currentTime,
            transform.position,
            transform.rotation,
            heldItem != null
        );

        // 들고 있는 아이템이 있다면 해당 아이템의 상태도 기록
        if (heldItem != null)
        {
            WarehouseItem warehouseItem = heldItem.GetComponent<WarehouseItem>();
            if (warehouseItem != null && ItemManager.Instance != null)
            {
                // ItemManager가 자동으로 아이템 상태를 기록함
                // 별도 호출 불필요 (ItemManager.Update에서 처리)
            }
        }
    }

    /// <summary>
    /// 아이템 홀드 포인트가 없다면 자동 생성
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
    /// 시간 정지 시 기록된 상태로 되돌림
    /// </summary>
    /// <param name="isPlaying">시간 재생 여부</param>
    private void OnTimeStateChanged(bool isPlaying)
    {
        this.isPlaying = isPlaying;
        if (!isPlaying)
        {
            ApplyRecordedState();
        }
    }

    /// <summary>
    /// 기록된 로봇 상태를 적용
    /// </summary>
    private void ApplyRecordedState()
    {
        if (RecordingManager.Instance == null || TimeManager.Instance == null) return;

        RobotState state = RecordingManager.Instance.GetRobotState(robotID, TimeManager.Instance.currentTime);
        if (state == null) return;

        // 위치와 회전 복원
        transform.position = state.position;
        transform.rotation = state.rotation;

        // 아이템 상태 복원은 ItemManager에서 처리
        // 여기서는 로봇의 heldItem 참조만 업데이트
        UpdateHeldItemReference();
    }

    /// <summary>
    /// 시간 복원 후 들고 있는 아이템 참조 업데이트
    /// </summary>
    public void UpdateHeldItemReference()
    {
        // 현재 홀드 포인트에 아이템이 있는지 확인
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
    /// 로봇 입력 활성화 설정
    /// </summary>
    /// <param name="active">활성화 여부</param>
    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>
    /// 지면 충돌 시 isGrounded 활성화
    /// </summary>
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    /// <summary>
    /// 지면 이탈 시 isGrounded 비활성화
    /// </summary>
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// 에디터에서 픽업 범위 및 아이템 위치 시각화
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
    /// 재생중 반환 메서드
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// 아이템 소지중 반환 메서드
    /// </summary>
    public bool HasItem()
    {
        return heldItem != null;
    }

    /// <summary>
    /// 로봇 선택 변경시에 이동변수를 초기화 하여 중복을 방지함
    /// </summary>
    public void ResetMovementState()
    {
        smoothVelocity = Vector3.zero;
        // 필요하다면 inputDirection, 기타 상태도 초기화
    }
}