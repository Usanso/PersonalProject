using UnityEngine;
using System;

/// <summary>
/// 역할: 게임 시간 흐름 제어, 재생/일시정지/리셋 기능_
/// 영향 관계: RobotController들의 움직임 제어, RecordingManager의 기록 재생 제어, UIManager의 시간바 업데이트_
/// 주요 기능: 시간 속도 조절, 시간 점프, 최대 시간 제한 관리)_
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("시간 설정")]
    [SerializeField] public float maxTime = 60f; // 전체 타임라인 길이
    [SerializeField] public float currentTime = 0f; // 현재 시간 (0 ~ maxTime) 
    public float playbackSpeed = 1f; // 재생 속도 (1 = 정방향, -1 = 역방향)
    public bool isPlaying = true;

    // 외부 시스템이 시간 변화에 반응하도록 이벤트 제공
    public event Action<bool> OnTimeUpdated;
    public event Action<float> OnTimeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        HandleInput();
        
        if (!isPlaying) return;

        UpdateTime();
        CheckTimeLimits();
        TriggerEvents();
    }

    /// <summary>
    /// 일시정지 조작키 스페이스바를 감지하는 함수
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

    /// <summary>
    /// 현재 시간을 업데이트 하는 함수
    /// </summary>
    private void UpdateTime()
    {
        currentTime += Time.deltaTime * playbackSpeed;
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);
    }

    /// <summary>
    /// 시간이 0미만 음수거나, 지정된 시간을 초과한 경우 자동으로 일시정지
    /// </summary>
    private void CheckTimeLimits()
    {
        if (currentTime >= maxTime || currentTime <= 0f)
        {
            Pause();
        }
    }

    /// <summary>
    /// 시간에 관여하는 이벤트 알람을 모은 함수
    /// </summary>
    private void TriggerEvents()
    {
        // 현재시간 알람
        OnTimeChanged?.Invoke(currentTime);
    }

    /// <summary>
    /// 일시정지 상태면 재생, 재생 상태면 일시정지 하는 토글
    /// </summary>
    public void TogglePlayPause()
    {
        if (isPlaying)
            Pause();
        else
            Play();
    }

    /// <summary>
    /// 시간 재생 시작
    /// </summary>
    public void Play(float speed = 1f)
    {
        playbackSpeed = speed;
        isPlaying = true;
        Time.timeScale = 1f; // 정상 시간 흐름
        OnTimeUpdated?.Invoke(isPlaying);
        MouseManager.Instance.LockCursor();
    }

    /// <summary>
    /// 시간 정지
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
        Time.timeScale = 0f; // Unity 전체 시간 정지
        OnTimeUpdated?.Invoke(isPlaying);
        MouseManager.Instance.UnlockCursor();

    }

    /// <summary>
    /// 시간 초기화
    /// </summary>
    public void ResetTime()
    {
        Pause();
        currentTime = 0f;
    }

    /// <summary>
    /// 특정 시간대로 점프
    /// </summary>
    /// <param name="targetTime">점프할 시간을 지정하는 변수</param>
    public void JumpToTime(float targetTime)
    {
        Pause();
        currentTime = Mathf.Clamp(targetTime, 0f, maxTime);
    }
}
