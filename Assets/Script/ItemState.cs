using UnityEngine;

/// <summary>
/// 아이템의 시간대별 위치, 상태를 저장하는 구조체
/// RecordingManager와 유사하지만 아이템 전용
/// </summary>
[System.Serializable]
public class ItemState
{
    public Vector3 position; // 아이템 위치
    public Quaternion rotation; // 아이템 회전
    public ItemCurrentState currentState; // 아이템 상태 (바닥/들림/정리완료)
    public int holdingRobotID; // 들고 있는 로봇 ID (-1이면 없음)

    /// <summary>
    /// 아이템 상태를 초기화하는 생성자
    /// </summary>
    /// <param name="pos">아이템 위치</param>
    /// <param name="rot">아이템 회전값</param>
    /// <param name="state">아이템 현재 상태</param>
    /// <param name="robotID">들고 있는 로봇 ID</param>
    public ItemState(Vector3 pos, Quaternion rot, ItemCurrentState state, int robotID)
    {
        position = pos;
        rotation = rot;
        currentState = state;
        holdingRobotID = robotID;
    }

    /// <summary>
    /// 빈 상태로 초기화하는 기본 생성자
    /// </summary>
    public ItemState()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        currentState = ItemCurrentState.OnGround;
        holdingRobotID = -1;
    }

    /// <summary>
    /// 현재 아이템 상태를 복사하여 새로운 ItemState 생성
    /// </summary>
    /// <param name="item">복사할 아이템</param>
    /// <returns>새로운 ItemState 객체</returns>
    public static ItemState CreateFromItem(WarehouseItem item)
    {
        return new ItemState(
            item.transform.position,
            item.transform.rotation,
            item.currentState,
            item.holdingRobotID
        );
    }

    /// <summary>
    /// 두 상태가 동일한지 비교
    /// </summary>
    /// <param name="other">비교할 다른 상태</param>
    /// <returns>동일하면 true</returns>
    public bool Equals(ItemState other)
    {
        if (other == null) return false;

        return Vector3.Distance(position, other.position) < 0.1f &&
               Quaternion.Angle(rotation, other.rotation) < 1f &&
               currentState == other.currentState &&
               holdingRobotID == other.holdingRobotID;
    }

    /// <summary>
    /// 디버그용 문자열 표현
    /// </summary>
    /// <returns>상태 정보 문자열</returns>
    public override string ToString()
    {
        return $"ItemState: Pos({position.x:F1}, {position.y:F1}, {position.z:F1}), " +
               $"State({currentState}), RobotID({holdingRobotID})";
    }
}