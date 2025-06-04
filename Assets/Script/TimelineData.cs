using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// TimelineData
/// 시간대별 로봇 상태를 저장하는 구조체
/// </summary>
[System.Serializable]
public class TimelineData
{
    public Vector3 position;           // 위치
    public Quaternion rotation;        // 회전
    public bool isHoldingItem;         // 아이템 보유 여부

    public TimelineData(Vector3 pos, Quaternion rot, bool holding)
    {
        position = pos;
        rotation = rot;
        isHoldingItem = holding;
    }
}
