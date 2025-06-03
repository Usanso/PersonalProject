using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 로봇의 특정 시점 행동 데이터를 담는 구조체
/// </summary>
[System.Serializable]
public struct RobotActionFrame
{
    public Vector3 position;      // 로봇 위치
    public Quaternion rotation;   // 로봇 회전
    public float timestamp;       // 행동 시점
    public bool isCarrying;       // 물건을 들고 있는지 여부
    public string actionType;     // "move", "pickup", "drop", "idle"
    public Vector3 carriedItemPosition; // 들고 있는 물건의 위치 (있다면)

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
/// 로봇의 행동을 녹화하고 재생하는 매니저 클래스
/// </summary>
public class RobotActionRecorder : MonoBehaviour
{
    [Header("녹화 설정")]
    [SerializeField] private float recordInterval = 0.1f; // 녹화 간격 (초)

    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;

    // 녹화된 행동 데이터 저장소
    private List<RobotActionFrame> recordedActions = new List<RobotActionFrame>();

    // 녹화/재생 상태 관리
    private bool isRecording = false;
    private bool isReplaying = false;
    private float lastRecordTime = 0f;
    private float replayStartTime = 0f;
    private int currentReplayIndex = 0;

    // 로봇 컴포넌트 참조
    private Transform robotTransform;
    private PlayerItemController robotController; // 로봇의 물건 들기/놓기 등을 담당하는 컴포넌트

    #region 초기화

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        robotTransform = transform;
        robotController = GetComponent<PlayerItemController>();
    }

    #endregion

    #region 녹화 관련 메서드


    /// <summary>
    /// 행동 녹화 시작
    /// </summary>
    public void StartRecording()
    {
        if (isReplaying)
        {
            Debug.LogWarning("재생 중에는 녹화를 시작할 수 없습니다.");
            return;
        }

        recordedActions.Clear();
        isRecording = true;
        lastRecordTime = Time.time;

        // 시작 시점의 로봇 상태 기록
        RecordCurrentState("start");

        if (showDebugInfo)
            Debug.Log("로봇 행동 녹화 시작");
    }

    /// <summary>
    /// 행동 녹화 중지
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;

        // 종료 시점의 로봇 상태 기록
        RecordCurrentState("end");

        if (showDebugInfo)
            Debug.Log($"로봇 행동 녹화 완료: {recordedActions.Count}개 프레임");
    }

    /// <summary>
    /// 현재 로봇 상태를 녹화 데이터에 기록
    /// </summary>
    /// <param name="actionType">행동 타입</param>
    private void RecordCurrentState(string actionType = "move")
    {
        if (!isRecording) return;

        Vector3 carriedItemPos = Vector3.zero;
        bool isCarrying = false;

        // 로봇이 물건을 들고 있는지 확인
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
    /// 정기적으로 로봇 상태 기록 (Update에서 호출)
    /// </summary>
    private void UpdateRecording()
    {
        if (!isRecording) return;

        // 지정된 간격마다 상태 기록
        if (Time.time - lastRecordTime >= recordInterval)
        {
            RecordCurrentState();
            lastRecordTime = Time.time;
        }
    }

    /// <summary>
    /// 특정 행동 발생 시 즉시 기록 (외부에서 호출)
    /// </summary>
    /// <param name="actionType">행동 타입 ("pickup", "drop" 등)</param>
    public void RecordSpecialAction(string actionType)
    {
        if (!isRecording) return;

        RecordCurrentState(actionType);

        if (showDebugInfo)
            Debug.Log($"특별 행동 기록: {actionType}");
    }

    /// <summary>
    /// 프로그래밍 방식으로 특정 위치로 이동 (재생 시 사용)
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    public void MoveToPosition(Vector3 targetPosition)
    {
        // 재생 시에는 물리 법칙 무시하고 직접 이동
        transform.position = targetPosition;

    }

    #endregion

    #region 재생 관련 메서드

    /// <summary>
    /// 녹화된 행동 재생 시작
    /// </summary>
    public void StartReplay()
    {
        if (isRecording)
        {
            Debug.LogWarning("녹화 중에는 재생을 시작할 수 없습니다.");
            return;
        }

        if (recordedActions.Count == 0)
        {
            Debug.LogWarning("재생할 녹화 데이터가 없습니다.");
            return;
        }

        isReplaying = true;
        replayStartTime = Time.time;
        currentReplayIndex = 0;

        // 시작 위치로 로봇 이동
        if (recordedActions.Count > 0)
        {
            ApplyActionFrame(recordedActions[0]);
        }

        if (showDebugInfo)
            Debug.Log("로봇 행동 재생 시작");
    }

    /// <summary>
    /// 행동 재생 중지
    /// </summary>
    public void StopReplay()
    {
        if (!isReplaying) return;

        isReplaying = false;
        currentReplayIndex = 0;

        if (showDebugInfo)
            Debug.Log("로봇 행동 재생 중지");
    }

    /// <summary>
    /// 재생 업데이트 (Update에서 호출)
    /// </summary>
    private void UpdateReplay()
    {
        if (!isReplaying || recordedActions.Count == 0) return;

        float currentReplayTime = Time.time - replayStartTime;

        // 현재 시간에 해당하는 액션 프레임 찾기
        while (currentReplayIndex < recordedActions.Count)
        {
            RobotActionFrame currentFrame = recordedActions[currentReplayIndex];
            float frameTime = currentFrame.timestamp - recordedActions[0].timestamp;

            if (frameTime <= currentReplayTime)
            {
                // 현재 프레임 적용
                ApplyActionFrame(currentFrame);
                currentReplayIndex++;

                // 특별 행동이 있다면 실행
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

        // 모든 프레임 재생 완료
        if (currentReplayIndex >= recordedActions.Count)
        {
            if (showDebugInfo)
                Debug.Log("로봇 행동 재생 완료");

            StopReplay();
        }
    }

    /// <summary>
    /// 액션 프레임을 로봇에 적용
    /// </summary>
    /// <param name="frame">적용할 액션 프레임</param>
    private void ApplyActionFrame(RobotActionFrame frame)
    {
        // 로봇 위치와 회전 적용
        robotTransform.position = frame.position;
        robotTransform.rotation = frame.rotation;

        // 물건을 들고 있는 상태 동기화
        if (robotController != null)
        {
            if (frame.isCarrying && frame.carriedItemPosition != Vector3.zero)
            {
                robotController.SetCarriedItemPosition(frame.carriedItemPosition);
            }
        }
    }

    /// <summary>
    /// 특별 행동 실행 (물건 들기/놓기 등)
    /// </summary>
    /// <param name="frame">실행할 액션 프레임</param>
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
            Debug.Log($"특별 행동 실행: {frame.actionType}");
    }

    /// <summary>
    /// 재생 시 픽업 행동 실행
    /// </summary>
    public void ExecutePickupAction()
    {
        //if (carriedItem == null)
        //{
        //    TryPickupItem();
        //}
    }

    /// <summary>
    /// 재생 시 드롭 행동 실행
    /// </summary>
    public void ExecuteDropAction()
    {
        //if (carriedItem != null)
        //{
        //    DropItem();
        //}
    }

    #endregion

    #region 상태 확인 메서드

    /// <summary>
    /// 현재 녹화 중인지 확인
    /// </summary>
    public bool IsRecording() => isRecording;

    /// <summary>
    /// 현재 재생 중인지 확인
    /// </summary>
    public bool IsReplaying() => isReplaying;

    /// <summary>
    /// 녹화된 데이터가 있는지 확인
    /// </summary>
    public bool HasRecordedData() => recordedActions.Count > 0;

    /// <summary>
    /// 녹화된 프레임 수 반환
    /// </summary>
    public int GetRecordedFrameCount() => recordedActions.Count;

    /// <summary>
    /// 녹화 데이터 초기화
    /// </summary>
    public void ClearRecordedData()
    {
        recordedActions.Clear();
        if (showDebugInfo)
            Debug.Log("녹화 데이터 초기화 완료");
    }

    #endregion

    #region Unity 생명주기

    private void Update()
    {
        UpdateRecording();
        UpdateReplay();
    }

    #endregion
}
