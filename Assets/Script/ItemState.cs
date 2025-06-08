using UnityEngine;

/// <summary>
/// �������� �ð��뺰 ��ġ, ���¸� �����ϴ� ����ü
/// RecordingManager�� ���������� ������ ����
/// </summary>
[System.Serializable]
public class ItemState
{
    public Vector3 position; // ������ ��ġ
    public Quaternion rotation; // ������ ȸ��
    public ItemCurrentState currentState; // ������ ���� (�ٴ�/�鸲/�����Ϸ�)
    public int holdingRobotID; // ��� �ִ� �κ� ID (-1�̸� ����)

    /// <summary>
    /// ������ ���¸� �ʱ�ȭ�ϴ� ������
    /// </summary>
    /// <param name="pos">������ ��ġ</param>
    /// <param name="rot">������ ȸ����</param>
    /// <param name="state">������ ���� ����</param>
    /// <param name="robotID">��� �ִ� �κ� ID</param>
    public ItemState(Vector3 pos, Quaternion rot, ItemCurrentState state, int robotID)
    {
        position = pos;
        rotation = rot;
        currentState = state;
        holdingRobotID = robotID;
    }

    /// <summary>
    /// �� ���·� �ʱ�ȭ�ϴ� �⺻ ������
    /// </summary>
    public ItemState()
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        currentState = ItemCurrentState.OnGround;
        holdingRobotID = -1;
    }

    /// <summary>
    /// ���� ������ ���¸� �����Ͽ� ���ο� ItemState ����
    /// </summary>
    /// <param name="item">������ ������</param>
    /// <returns>���ο� ItemState ��ü</returns>
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
    /// �� ���°� �������� ��
    /// </summary>
    /// <param name="other">���� �ٸ� ����</param>
    /// <returns>�����ϸ� true</returns>
    public bool Equals(ItemState other)
    {
        if (other == null) return false;

        return Vector3.Distance(position, other.position) < 0.1f &&
               Quaternion.Angle(rotation, other.rotation) < 1f &&
               currentState == other.currentState &&
               holdingRobotID == other.holdingRobotID;
    }

    /// <summary>
    /// ����׿� ���ڿ� ǥ��
    /// </summary>
    /// <returns>���� ���� ���ڿ�</returns>
    public override string ToString()
    {
        return $"ItemState: Pos({position.x:F1}, {position.y:F1}, {position.z:F1}), " +
               $"State({currentState}), RobotID({holdingRobotID})";
    }
}