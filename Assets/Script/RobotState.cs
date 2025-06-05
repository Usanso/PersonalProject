using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �κ��� �ð��뺰 ��ġ, ���¸� �����ϴ� ����ü
/// </summary>
[System.Serializable]
public class RobotState
{
    public Vector3 position;           // ��ġ
    public Quaternion rotation;        // ȸ��
    public bool hasItem;         // ������ ���� ����

    /// <summary>
    /// �κ� ���¸� �ʱ�ȭ�ϴ� ������
    /// </summary>
    /// <param name="pos">�κ� ��ġ</param>
    /// <param name="rot">�κ� ȸ����</param>
    /// <param name="holding">������ ���� ����</param>
    public RobotState(Vector3 pos, Quaternion rot, bool holding)
    {
        position = pos;
        rotation = rot;
        hasItem = holding;
    }
}
