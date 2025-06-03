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
    public float maxTime = 10f;         // 전체 타임라인 길이
    public float currentTime = 0f;      // 현재 시간 (0 ~ maxTime)
    public float playbackSpeed = 1f;    // 재생 속도 (1 = 정방향, -1 = 역방향)

    [Header("시간 상태")]
    public bool isPlaying = false;

    // 외부 시스템이 시간 변화에 반응하도록 이벤트 제공
    public Action<float> OnTimeUpdated;

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
        if (!isPlaying) return;

        // 시간 진행
        currentTime += Time.deltaTime * playbackSpeed;

        // 범위 제한
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        // 시간 업데이트 이벤트 실행
        OnTimeUpdated?.Invoke(currentTime);

        // 시간이 끝에 도달하면 정지
        if (currentTime >= maxTime || currentTime <= 0f)
        {
            Pause();
        }
    }

    /// <summary>
    /// 시간 재생 시작
    /// </summary>
    public void Play(float speed = 1f)
    {
        playbackSpeed = speed;
        isPlaying = true;
    }

    /// <summary>
    /// 시간 정지
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// 시간 초기화
    /// </summary>
    public void ResetTime()
    {
        Pause();
        currentTime = 0f;
        OnTimeUpdated?.Invoke(currentTime);
    }

    /// <summary>
    /// 특정 시간대로 점프
    /// </summary>
    public void JumpTo(float targetTime)
    {
        Pause();
        currentTime = Mathf.Clamp(targetTime, 0f, maxTime);
        OnTimeUpdated?.Invoke(currentTime);
    }

    /// <summary>
    /// 과거로 되감기
    /// </summary>
    public void RewindTo(float targetTime)
    {
        if (targetTime < currentTime)
        {
            Pause();
            currentTime = Mathf.Clamp(targetTime, 0f, maxTime);
            OnTimeUpdated?.Invoke(currentTime);
        }
    }
}
