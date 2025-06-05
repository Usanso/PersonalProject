using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로봇의 시간대별 위치, 상태를 저장하는 구조체
/// </summary>
[System.Serializable]
public class RobotState
{
    public Vector3 position;           // 위치
    public Quaternion rotation;        // 회전
    public bool hasItem;         // 아이템 보유 여부

    /// <summary>
    /// 로봇 상태를 초기화하는 생성자
    /// </summary>
    /// <param name="pos">로봇 위치</param>
    /// <param name="rot">로봇 회전값</param>
    /// <param name="holding">아이템 보유 여부</param>
    public RobotState(Vector3 pos, Quaternion rot, bool holding)
    {
        position = pos;
        rotation = rot;
        hasItem = holding;
    }
}
