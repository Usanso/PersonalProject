using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RecordingManager
/// 모든 로봇의 행동을 시간대별로 기록하고 재생
/// </summary>
public class RecordingManager : MonoBehaviour
{
    public static RecordingManager Instance { get; private set; }

    private Dictionary<int, Dictionary<float, TimelineData>> robotRecords = new();

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
    /// 특정 로봇의 현재 상태를 시간에 따라 기록 (일시정지에서 재생하면 그 뒤 기록은 없어지도록 설정 필요)
    /// </summary>
    public void Record(int robotID, float time, Vector3 pos, Quaternion rot, bool holding)
    {
        if (!robotRecords.ContainsKey(robotID))
        {
            robotRecords[robotID] = new Dictionary<float, TimelineData>();
        }

        robotRecords[robotID][time] = new TimelineData(pos, rot, holding);
    }

    /// <summary>
    /// 특정 로봇의 지정된 시간대 상태 반환
    /// </summary>
    public TimelineData GetStateAt(int robotID, float time)
    {
        if (robotRecords.ContainsKey(robotID) && robotRecords[robotID].ContainsKey(time))
        {
            return robotRecords[robotID][time];
        }
        return null;
    }

    /// <summary>
    /// 로봇의 기록된 상태를 현재 시간대로 재생
    /// </summary>
    public void ApplyState(int robotID, float time, Transform targetTransform, out bool isHolding)
    {
        var data = GetStateAt(robotID, time);
        if (data != null)
        {
            targetTransform.position = data.position;
            targetTransform.rotation = data.rotation;
            isHolding = data.isHoldingItem;
        }
        else
        {
            isHolding = false;
        }
    }

    /// <summary>
    /// 모든 기록 초기화 (예: 리셋 시)
    /// </summary>
    public void ClearAllRecords()
    {
        robotRecords.Clear();
    }
}

