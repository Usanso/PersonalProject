using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// TimelineData
/// �ð��뺰 �κ� ���¸� �����ϴ� ����ü
/// </summary>
[System.Serializable]
public class TimelineData
{
    public Vector3 position;           // ��ġ
    public Quaternion rotation;        // ȸ��
    public bool isHoldingItem;         // ������ ���� ����

    public TimelineData(Vector3 pos, Quaternion rot, bool holding)
    {
        position = pos;
        rotation = rot;
        isHoldingItem = holding;
    }
}
