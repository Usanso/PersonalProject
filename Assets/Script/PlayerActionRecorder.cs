using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �κ��� Ư�� ���� �ൿ �����͸� ��� ����ü
/// </summary>
[System.Serializable]
public struct RobotActionFrame
{
    public Vector3 position;      // �κ� ��ġ
    public Quaternion rotation;   // �κ� ȸ��
    public float timestamp;       // �ൿ ����
    public bool isCarrying;       // ������ ��� �ִ��� ����
    public string actionType;     // "move", "pickup", "drop", "idle"
    public Vector3 carriedItemPosition; // ��� �ִ� ������ ��ġ (�ִٸ�)

    public RobotActionFrame(Vector3 pos, Quaternion rot, float time, bool carrying, string action, Vector3 itemPos = default)
    {
        position = pos;
        rotation = rot;
        timestamp = time;
        isCarrying = carrying;
        actionType = action;
        carriedItemPosition = itemPos;
    }
}

/// <summary>
/// �κ��� �ൿ�� ��ȭ�ϰ� ����ϴ� �Ŵ��� Ŭ����
/// </summary>
public class RobotActionRecorder : MonoBehaviour
{
    [Header("��ȭ ����")]
    [SerializeField] private float recordInterval = 0.1f; // ��ȭ ���� (��)

    [Header("�����")]
    [SerializeField] private bool showDebugInfo = true;

    // ��ȭ�� �ൿ ������ �����
    private List<RobotActionFrame> recordedActions = new List<RobotActionFrame>();

    // ��ȭ/��� ���� ����
    private bool isRecording = false;
    private bool isReplaying = false;
    private float lastRecordTime = 0f;
    private float replayStartTime = 0f;
    private int currentReplayIndex = 0;

    // �κ� ������Ʈ ����
    private Transform robotTransform;
    private PlayerItemController robotController; // �κ��� ���� ���/���� ���� ����ϴ� ������Ʈ

    #region �ʱ�ȭ

    /// <summary>
    /// ������Ʈ �ʱ�ȭ
    /// </summary>
    private void Awake()
    {
        robotTransform = transform;
        robotController = GetComponent<PlayerItemController>();
    }

    #endregion

    #region ��ȭ ���� �޼���


    /// <summary>
    /// �ൿ ��ȭ ����
    /// </summary>
    public void StartRecording()
    {
        if (isReplaying)
        {
            Debug.LogWarning("��� �߿��� ��ȭ�� ������ �� �����ϴ�.");
            return;
        }

        recordedActions.Clear();
        isRecording = true;
        lastRecordTime = Time.time;

        // ���� ������ �κ� ���� ���
        RecordCurrentState("start");

        if (showDebugInfo)
            Debug.Log("�κ� �ൿ ��ȭ ����");
    }

    /// <summary>
    /// �ൿ ��ȭ ����
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;

        // ���� ������ �κ� ���� ���
        RecordCurrentState("end");

        if (showDebugInfo)
            Debug.Log($"�κ� �ൿ ��ȭ �Ϸ�: {recordedActions.Count}�� ������");
    }

    /// <summary>
    /// ���� �κ� ���¸� ��ȭ �����Ϳ� ���
    /// </summary>
    /// <param name="actionType">�ൿ Ÿ��</param>
    private void RecordCurrentState(string actionType = "move")
    {
        if (!isRecording) return;

        Vector3 carriedItemPos = Vector3.zero;
        bool isCarrying = false;

        // �κ��� ������ ��� �ִ��� Ȯ��
        if (robotController != null)
        {
            isCarrying = robotController.IsCarryingItem();
            if (isCarrying)
                carriedItemPos = robotController.GetCarriedItemPosition();
        }

        RobotActionFrame frame = new RobotActionFrame(
            robotTransform.position,
            robotTransform.rotation,
            Time.time,
            isCarrying,
            actionType,
            carriedItemPos
        );

        recordedActions.Add(frame);
    }

    /// <summary>
    /// ���������� �κ� ���� ��� (Update���� ȣ��)
    /// </summary>
    private void UpdateRecording()
    {
        if (!isRecording) return;

        // ������ ���ݸ��� ���� ���
        if (Time.time - lastRecordTime >= recordInterval)
        {
            RecordCurrentState();
            lastRecordTime = Time.time;
        }
    }

    /// <summary>
    /// Ư�� �ൿ �߻� �� ��� ��� (�ܺο��� ȣ��)
    /// </summary>
    /// <param name="actionType">�ൿ Ÿ�� ("pickup", "drop" ��)</param>
    public void RecordSpecialAction(string actionType)
    {
        if (!isRecording) return;

        RecordCurrentState(actionType);

        if (showDebugInfo)
            Debug.Log($"Ư�� �ൿ ���: {actionType}");
    }

    /// <summary>
    /// ���α׷��� ������� Ư�� ��ġ�� �̵� (��� �� ���)
    /// </summary>
    /// <param name="targetPosition">��ǥ ��ġ</param>
    public void MoveToPosition(Vector3 targetPosition)
    {
        // ��� �ÿ��� ���� ��Ģ �����ϰ� ���� �̵�
        transform.position = targetPosition;

    }

    #endregion

    #region ��� ���� �޼���

    /// <summary>
    /// ��ȭ�� �ൿ ��� ����
    /// </summary>
    public void StartReplay()
    {
        if (isRecording)
        {
            Debug.LogWarning("��ȭ �߿��� ����� ������ �� �����ϴ�.");
            return;
        }

        if (recordedActions.Count == 0)
        {
            Debug.LogWarning("����� ��ȭ �����Ͱ� �����ϴ�.");
            return;
        }

        isReplaying = true;
        replayStartTime = Time.time;
        currentReplayIndex = 0;

        // ���� ��ġ�� �κ� �̵�
        if (recordedActions.Count > 0)
        {
            ApplyActionFrame(recordedActions[0]);
        }

        if (showDebugInfo)
            Debug.Log("�κ� �ൿ ��� ����");
    }

    /// <summary>
    /// �ൿ ��� ����
    /// </summary>
    public void StopReplay()
    {
        if (!isReplaying) return;

        isReplaying = false;
        currentReplayIndex = 0;

        if (showDebugInfo)
            Debug.Log("�κ� �ൿ ��� ����");
    }

    /// <summary>
    /// ��� ������Ʈ (Update���� ȣ��)
    /// </summary>
    private void UpdateReplay()
    {
        if (!isReplaying || recordedActions.Count == 0) return;

        float currentReplayTime = Time.time - replayStartTime;

        // ���� �ð��� �ش��ϴ� �׼� ������ ã��
        while (currentReplayIndex < recordedActions.Count)
        {
            RobotActionFrame currentFrame = recordedActions[currentReplayIndex];
            float frameTime = currentFrame.timestamp - recordedActions[0].timestamp;

            if (frameTime <= currentReplayTime)
            {
                // ���� ������ ����
                ApplyActionFrame(currentFrame);
                currentReplayIndex++;

                // Ư�� �ൿ�� �ִٸ� ����
                if (currentFrame.actionType == "pickup" || currentFrame.actionType == "drop")
                {
                    ExecuteSpecialAction(currentFrame);
                }
            }
            else
            {
                break;
            }
        }

        // ��� ������ ��� �Ϸ�
        if (currentReplayIndex >= recordedActions.Count)
        {
            if (showDebugInfo)
                Debug.Log("�κ� �ൿ ��� �Ϸ�");

            StopReplay();
        }
    }

    /// <summary>
    /// �׼� �������� �κ��� ����
    /// </summary>
    /// <param name="frame">������ �׼� ������</param>
    private void ApplyActionFrame(RobotActionFrame frame)
    {
        // �κ� ��ġ�� ȸ�� ����
        robotTransform.position = frame.position;
        robotTransform.rotation = frame.rotation;

        // ������ ��� �ִ� ���� ����ȭ
        if (robotController != null)
        {
            if (frame.isCarrying && frame.carriedItemPosition != Vector3.zero)
            {
                robotController.SetCarriedItemPosition(frame.carriedItemPosition);
            }
        }
    }

    /// <summary>
    /// Ư�� �ൿ ���� (���� ���/���� ��)
    /// </summary>
    /// <param name="frame">������ �׼� ������</param>
    private void ExecuteSpecialAction(RobotActionFrame frame)
    {
        if (robotController == null) return;

        switch (frame.actionType)
        {
            case "pickup":
                ExecutePickupAction();
                break;
            case "drop":
                ExecuteDropAction();
                break;
        }

        if (showDebugInfo)
            Debug.Log($"Ư�� �ൿ ����: {frame.actionType}");
    }

    /// <summary>
    /// ��� �� �Ⱦ� �ൿ ����
    /// </summary>
    public void ExecutePickupAction()
    {
        //if (carriedItem == null)
        //{
        //    TryPickupItem();
        //}
    }

    /// <summary>
    /// ��� �� ��� �ൿ ����
    /// </summary>
    public void ExecuteDropAction()
    {
        //if (carriedItem != null)
        //{
        //    DropItem();
        //}
    }

    #endregion

    #region ���� Ȯ�� �޼���

    /// <summary>
    /// ���� ��ȭ ������ Ȯ��
    /// </summary>
    public bool IsRecording() => isRecording;

    /// <summary>
    /// ���� ��� ������ Ȯ��
    /// </summary>
    public bool IsReplaying() => isReplaying;

    /// <summary>
    /// ��ȭ�� �����Ͱ� �ִ��� Ȯ��
    /// </summary>
    public bool HasRecordedData() => recordedActions.Count > 0;

    /// <summary>
    /// ��ȭ�� ������ �� ��ȯ
    /// </summary>
    public int GetRecordedFrameCount() => recordedActions.Count;

    /// <summary>
    /// ��ȭ ������ �ʱ�ȭ
    /// </summary>
    public void ClearRecordedData()
    {
        recordedActions.Clear();
        if (showDebugInfo)
            Debug.Log("��ȭ ������ �ʱ�ȭ �Ϸ�");
    }

    #endregion

    #region Unity �����ֱ�

    private void Update()
    {
        UpdateRecording();
        UpdateReplay();
    }

    #endregion
}
