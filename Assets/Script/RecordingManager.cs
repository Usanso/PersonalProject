using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 로봇의 행동을 시간대별로 기록하고 재생
/// 시간대에 가지고 있던 물리력도 저장
/// 아이템 줍기 놓기 행동도 기록
/// 행동 기록 프레임
/// </summary>
public class RecordingManager : MonoBehaviour
{
    public static RecordingManager Instance { get; private set; }

    [Header("녹화 설정")]
    [SerializeField] private float recordInterval = 0.1f; // 녹화 간격 (초)

    /// <summary>
    /// 전체 로봇 상태 기록 데이터, 로봇ID → 시간 → 로봇상태
    /// </summary>
    private Dictionary<int, Dictionary<float, RobotState>> allRecords = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 로봇의 상태를 특정 시간에 기록, 미래 시간이 이미 기록돼 있다면 해당 기록은 제거
    /// </summary>
    /// <param name="robotID">로봇 고유 ID</param>
    /// <param name="time">기록할 시간</param>
    /// <param name="position">위치</param>
    /// <param name="rotation">회전</param>
    /// <param name="hasItem">아이템 보유 여부</param>
    public void RecordRobotState(int robotID, float time, Vector3 position, Quaternion rotation, bool hasItem)
    {
        // 해당 로봇 ID가 최초로 나타난 경우 기록하려다가 오류 발생함, 때문에 방지를 위해 먼저 없다면 딕셔너리 칸을 생성
        if (!allRecords.ContainsKey(robotID))
        {
            allRecords[robotID] = new Dictionary<float, RobotState>();
        }

        // 새로운 기록을 할 떄 미래 기록을 지움
        ClearFutureRecords(robotID, time);

        // 현재 상태 기록
        allRecords[robotID][time] = new RobotState(position, rotation, hasItem);
    }

    /// <summary>
    /// 특정 로봇의 지정된 시간대 상태 반환
    /// </summary>
    /// <param name="robotID">로봇 ID</param>
    /// <param name="time">시간대</param>
    /// <returns> RobotState (없으면 null) </returns>
    public RobotState GetRobotState(int robotID, float time)
    {
        // id 존재 확인
        if (allRecords.ContainsKey(robotID))
        {
            // 가장 가까운 이전 시점을 찾음
            float closestTime = FindClosestTime(robotID, time);
            // 해당 시점의 상태를 반환
            if (allRecords[robotID].ContainsKey(closestTime))
            {
                return allRecords[robotID][closestTime];
            }
        }
        return null;
    }

    /// <summary>
    /// 주어진 시간보다 작거나 같은 가장 가까운 시간을 찾아 반환합니다. 시간 단위 맞추기용 (조정하며 수정 필요)
    /// </summary>
    /// <param name="robotID">로봇 고유 ID</param>
    /// <param name="targetTime">찾을 기준 시간</param>
    /// <returns>가장 가까운 시간 (존재하지 않으면 0)</returns>
    private float FindClosestTime(int robotID, float targetTime)
    {
        float closestTime = 0f;
        float minDifference = float.MaxValue;

        foreach (float recordedTime in allRecords[robotID].Keys)
        {
            float difference = Mathf.Abs(recordedTime - targetTime);
            if (difference < minDifference && recordedTime <= targetTime)
            {
                minDifference = difference;
                closestTime = recordedTime;
            }
        }

        return closestTime;
    }

    /// <summary>
    /// 현재 시간 이후로 기록된 미래 시점 데이터를 제거합니다.
    /// </summary>
    /// <param name="robotID">로봇 고유 ID</param>
    /// <param name="currentTime">기준 시간</param>
    private void ClearFutureRecords(int robotID, float currentTime)
    {
        // ID 존재 확인
        if (!allRecords.ContainsKey(robotID)) return;

        // 현재 시간을 초과한 시간을 담기 위한 리스트 변수
        List<float> timesToRemove = new List<float>();

        // 기존 딕셔너리에서 현재시간 이상만 순회하여 찾음
        foreach (float time in allRecords[robotID].Keys)
        {
            if (time > currentTime)
            {
                timesToRemove.Add(time);
            }
        }

        // 찾은 시간을 삭제
        foreach (float time in timesToRemove)
        {
            allRecords[robotID].Remove(time);
        }
    }

    /// <summary>
    /// 모든 로봇id, 시간, 상태 기록 초기화
    /// </summary>
    public void ClearAllRecords()
    {
        allRecords.Clear();
    }

    /// <summary>
    /// 특정 로봇의 모든 기록을 초기화합니다.
    /// </summary>
    /// <param name="robotID">로봇 고유 ID</param>
    public void ClearRobotRecords(int robotID)
    {
        if (allRecords.ContainsKey(robotID))
        {
            allRecords[robotID].Clear();
        }
    }
}

