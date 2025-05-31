using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로봇의 물건 조작, (녹화 및 재생이 일부 남아있음) 담당하는 컨트롤러
/// </summary>
public class PlayerItemController : MonoBehaviour
{
    // 컴포넌트 참조
    private Rigidbody rb;
    private RobotActionRecorder actionRecorder;

    [Header("물건 조작 설정")]
    [SerializeField] private Transform itemHoldPoint; // 물건을 들 위치
    [SerializeField] private float pickupRange = 1.5f; // 물건을 들 수 있는 거리
    private LayerMask itemLayerMask = 128; // 물건 레이어

    [Header("디버그")]
    [SerializeField] private bool showDebugGizmos = true;

    // 상태 변수
    [SerializeField] private bool isPlayerControlled = false; // 플레이어가 조작 중인지
    private GameObject carriedItem = null; // 현재 들고 있는 물건

    #region 초기화
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // actionRecorder = GetComponent<RobotActionRecorder>();

        if (itemHoldPoint == null)
        {
            GameObject holdPoint = new GameObject("ItemHoldPoint");
            holdPoint.transform.SetParent(transform);
            holdPoint.transform.localPosition = Vector3.up * 0.5f; // 들고 있는 물건 위치 (로봇 위쪽)
            itemHoldPoint = holdPoint.transform;
        }
    }
    #endregion

    #region Unity 생명주기 및 기즈모
    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 디버그 기즈모 그리기
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // 픽업 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        // 아이템 홀드 포인트
        if (itemHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemHoldPoint.position, 0.2f);
        }
    }
    #endregion

    #region 플레이어 입력 처리

    /// <summary>
    /// 플레이어 조작 모드 설정 (후추)
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

        // 스페이스바로 물건 들기/놓기
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleItemInteraction();
        }
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
    }

    /// <summary>
    /// 물건 놓기
    /// </summary>
    private void DropItem()
    {
        if (carriedItem == null) return;

        // 놓을 위치 계산 (로봇 앞쪽)
        Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 1f;

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

        // 녹화 중이라면 드롭 행동 기록
        if (actionRecorder != null && actionRecorder.IsRecording())
        {
            actionRecorder.RecordSpecialAction("drop");
        }

        carriedItem = null;
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
}
