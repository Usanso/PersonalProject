using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로봇의 이동, 물건 조작, 충돌 처리를 담당하는 컨트롤러
/// </summary>
public class RobotController : MonoBehaviour
{
    // 컴포넌트 참조
    private Rigidbody rb;
    private RobotActionRecorder actionRecorder;

    // 충돌 감지용
    private Collider robotCollider;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 8f;

    [SerializeField] private float deceleration = 15f; // 감속도
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float rotationAcceleration = 720f; // 회전 가속도

    [Header("물건 조작 설정")]
    [SerializeField] private Transform itemHoldPoint; // 물건을 들 위치
    [SerializeField] private float pickupRange = 1.5f; // 물건을 들 수 있는 거리
    [SerializeField] private LayerMask itemLayerMask = 1; // 물건 레이어

    [Header("충돌 설정")]
    [SerializeField] private float robotRadius = 0.5f; // 로봇 충돌 반지름
    [SerializeField] private LayerMask obstacleLayerMask = 1; // 장애물 레이어

    [Header("디버그")]
    [SerializeField] private bool showDebugGizmos = true;

    // 상태 변수
    [SerializeField] private bool isPlayerControlled = false; // 플레이어가 조작 중인지
    private GameObject carriedItem = null; // 현재 들고 있는 물건
    private Vector3 moveInput = Vector3.zero; // 입력 벡터
    private Vector3 currentVelocity = Vector3.zero; // 현재 속도
    private float currentRotationVelocity = 0f; // 현재 회전 속도

    #region 초기화

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // actionRecorder = GetComponent<RobotActionRecorder>();
        robotCollider = GetComponent<Collider>();

        //// Rigidbody 설정
        //if (rb != null)
        //{
        //    rb.freezeRotation = true; // 물리적 회전 고정 (스크립트로 제어)
        //    rb.drag = 8f; // 저항 증가로 미끄러짐 방지
        //    rb.angularDrag = 10f; // 회전 저항
        //    rb.mass = 2f; // 질량 증가로 안정성 향상

        //    // 무게중심을 낮춰서 넘어지지 않게 설정
        //    rb.centerOfMass = new Vector3(0, -0.5f, 0);
        //}

        // 아이템 홀드 포인트가 없다면 자동 생성
        if (itemHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("ItemHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = Vector3.up * 1.5f; // 로봇 위쪽
            itemHoldPoint = holdPoint.transform;
        }
    }

    #endregion

    #region 플레이어 입력 처리

    /// <summary>
    /// 플레이어 조작 모드 설정
    /// </summary>
    /// <param name="controlled">플레이어가 조작할지 여부</param>
    public void SetPlayerControlled(bool controlled)
    {
        isPlayerControlled = controlled;

        if (controlled && actionRecorder != null)
        {
            // 플레이어 조작 시작 시 녹화 시작
            actionRecorder.StartRecording();
        }
        else if (!controlled && actionRecorder != null)
        {
            // 플레이어 조작 종료 시 녹화 중지
            actionRecorder.StopRecording();
        }
    }

    /// <summary>
    /// 입력 처리 (Update에서 호출)
    /// </summary>
    private void HandleInput()
    {
        if (!isPlayerControlled) return;

        // WASD 또는 화살표 키로 이동
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveInput = new Vector3(horizontal, 0, vertical).normalized;

        // 스페이스바로 물건 들기/놓기
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleItemInteraction();
        }
    }

    #endregion

    #region 이동 처리

    /// <summary>
    /// 로봇 이동 처리 (즉각 반응하는 가속/감속 방식)
    /// </summary>
    private void HandleMovement()
    {
        // 이동 처리 (속도 기반)
        rb.velocity = moveInput * moveSpeed + new Vector3(0, rb.velocity.y, 0);

        // 이동 적용 (충돌 검사 포함)
        if (currentVelocity.magnitude > 0.01f)
        {
            Vector3 targetPosition = transform.position + currentVelocity * Time.fixedDeltaTime;

            if (CanMoveTo(targetPosition))
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                // 충돌 시 해당 방향 속도 제거
                Vector3 moveDirection = (targetPosition - transform.position).normalized;
                currentVelocity = Vector3.ProjectOnPlane(currentVelocity, moveDirection);
            }
        }
    }

    /// <summary>
    /// 특정 위치로 이동 가능한지 검사
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <returns>이동 가능 여부</returns>
    private bool CanMoveTo(Vector3 targetPosition)
    {
        // 구체 형태로 충돌 검사
        Collider[] obstacles = Physics.OverlapSphere(targetPosition, robotRadius, obstacleLayerMask);

        // 자기 자신은 제외
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
    /// 프로그래밍 방식으로 특정 위치로 이동 (재생 시 사용)
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    public void MoveToPosition(Vector3 targetPosition)
    {
        // 재생 시에는 물리 법칙 무시하고 직접 이동
        transform.position = targetPosition;

        // 재생 중에는 속도 초기화
        currentVelocity = Vector3.zero;
        currentRotationVelocity = 0f;
    }

    #endregion

    #region 물건 조작

    /// <summary>
    /// 물건 상호작용 처리 (들기/놓기)
    /// </summary>
    private void HandleItemInteraction()
    {
        if (carriedItem == null)
        {
            // 물건 들기 시도
            TryPickupItem();
        }
        else
        {
            // 물건 놓기
            DropItem();
        }
    }

    /// <summary>
    /// 주변 물건 들기 시도
    /// </summary>
    private void TryPickupItem()
    {
        // 주변 물건 검색
        Collider[] nearbyItems = Physics.OverlapSphere(transform.position, pickupRange, itemLayerMask);

        GameObject closestItem = null;
        float closestDistance = float.MaxValue;

        // 가장 가까운 물건 찾기
        foreach (var itemCollider in nearbyItems)
        {
            float distance = Vector3.Distance(transform.position, itemCollider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = itemCollider.gameObject;
            }
        }

        // 물건 들기 실행
        if (closestItem != null)
        {
            PickupItem(closestItem);
        }
    }

    /// <summary>
    /// 물건 들기 실행
    /// </summary>
    /// <param name="item">들 물건</param>
    private void PickupItem(GameObject item)
    {
        carriedItem = item;

        // 물건을 홀드 포인트로 이동
        item.transform.SetParent(itemHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        // 물리 비활성화
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = true;
        }

        // 충돌 비활성화
        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }

        // 녹화 중이라면 픽업 행동 기록
        if (actionRecorder != null && actionRecorder.IsRecording())
        {
            actionRecorder.RecordSpecialAction("pickup");
        }

        Debug.Log($"물건 픽업: {item.name}");
    }

    /// <summary>
    /// 물건 놓기
    /// </summary>
    private void DropItem()
    {
        if (carriedItem == null) return;

        // 놓을 위치 계산 (로봇 앞쪽)
        Vector3 dropPosition = transform.position + transform.forward * 1.5f;

        // 물건을 월드로 이동
        carriedItem.transform.SetParent(null);
        carriedItem.transform.position = dropPosition;

        // 물리 활성화
        Rigidbody itemRb = carriedItem.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = false;
        }

        // 충돌 활성화
        Collider itemCollider = carriedItem.GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.isTrigger = false;
        }

        Debug.Log($"물건 드롭: {carriedItem.name}");

        // 녹화 중이라면 드롭 행동 기록
        if (actionRecorder != null && actionRecorder.IsRecording())
        {
            actionRecorder.RecordSpecialAction("drop");
        }

        carriedItem = null;
    }

    /// <summary>
    /// 재생 시 픽업 행동 실행
    /// </summary>
    public void ExecutePickupAction()
    {
        if (carriedItem == null)
        {
            TryPickupItem();
        }
    }

    /// <summary>
    /// 재생 시 드롭 행동 실행
    /// </summary>
    public void ExecuteDropAction()
    {
        if (carriedItem != null)
        {
            DropItem();
        }
    }

    #endregion

    #region 상태 확인 메서드

    /// <summary>
    /// 물건을 들고 있는지 확인
    /// </summary>
    /// <returns>물건 소지 여부</returns>
    public bool IsCarryingItem()
    {
        return carriedItem != null;
    }

    /// <summary>
    /// 들고 있는 물건의 위치 반환
    /// </summary>
    /// <returns>물건 위치</returns>
    public Vector3 GetCarriedItemPosition()
    {
        if (carriedItem != null)
            return carriedItem.transform.position;
        return Vector3.zero;
    }

    /// <summary>
    /// 들고 있는 물건의 위치 설정 (재생 시 사용)
    /// </summary>
    /// <param name="position">설정할 위치</param>
    public void SetCarriedItemPosition(Vector3 position)
    {
        if (carriedItem != null)
        {
            carriedItem.transform.position = position;
        }
    }

    /// <summary>
    /// 플레이어 조작 중인지 확인
    /// </summary>
    /// <returns>플레이어 조작 여부</returns>
    public bool IsPlayerControlled()
    {
        return isPlayerControlled;
    }

    #endregion

    #region 충돌 감지

    /// <summary>
    /// 다른 로봇과 충돌 시 호출
    /// </summary>
    /// <param name="other">충돌한 객체</param>
    private void OnTriggerEnter(Collider other)
    {
        // 다른 로봇과 충돌한 경우
        RobotController otherRobot = other.GetComponent<RobotController>();
        if (otherRobot != null)
        {
            Debug.LogWarning($"{gameObject.name}이 {other.gameObject.name}과 충돌!");

            // 게임 매니저에 충돌 알림 (추후 구현)
            // GameManager.Instance?.OnRobotCollision(this, otherRobot);
        }
    }

    #endregion

    #region Unity 생명주기

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// 디버그 기즈모 그리기
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // 로봇 충돌 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, robotRadius);

        // 픽업 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        // 이동 방향 (현재 속도 기준)
        if (currentVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, currentVelocity.normalized * 2f);
        }

        // 아이템 홀드 포인트
        if (itemHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemHoldPoint.position, 0.2f);
        }
    }

    #endregion
}
