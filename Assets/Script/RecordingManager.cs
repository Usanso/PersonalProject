using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RecordingManager
/// ��� �κ��� �ൿ�� �ð��뺰�� ����ϰ� ���
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
    /// Ư�� �κ��� ���� ���¸� �ð��� ���� ��� (�Ͻ��������� ����ϸ� �� �� ����� ���������� ���� �ʿ�)
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
    /// Ư�� �κ��� ������ �ð��� ���� ��ȯ
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
    /// �κ��� ��ϵ� ���¸� ���� �ð���� ���
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
    /// ��� ��� �ʱ�ȭ (��: ���� ��)
    /// </summary>
    public void ClearAllRecords()
    {
        robotRecords.Clear();
    }
}

