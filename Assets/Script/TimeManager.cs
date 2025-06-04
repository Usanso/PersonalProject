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
    [SerializeField] public float maxTime = 10f;         // 전체 타임라인 길이
    [SerializeField] public float currentTime = 0f;      // 현재 시간 (0 ~ maxTime) 
    public float playbackSpeed = 1f;    // 재생 속도 (1 = 정방향, -1 = 역방향)

    [Header("시간 상태")]
    public bool isPlaying = true;

    // 외부 시스템이 시간 변화에 반응하도록 이벤트 제공
    public event Action<bool> OnTimeUpdated;

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
        PlayButtonInput();
        if (!isPlaying) return;

        // 시간 진행
        currentTime += Time.deltaTime * playbackSpeed;

        // 범위 제한
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        // 시간 업데이트 이벤트 실행
        OnTimeUpdated?.Invoke(isPlaying);

        TimeMaxCheck();
    }

    public void PlayButtonInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
    }

    /// <summary>
    /// 시간이 0미만 음수거나, 지정된 시간을 초과한 경우 자동으로 일시정지
    /// </summary>
    public void TimeMaxCheck()
    {
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
        Time.timeScale = 1f; // 정상 시간 흐름
        OnTimeUpdated?.Invoke(true);
        CursorUtils.LockCursor();
    }

    /// <summary>
    /// 시간 정지
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
        Time.timeScale = 0f; // Unity 전체 시간 정지
        OnTimeUpdated?.Invoke(false);
        CursorUtils.UnlockCursor();

    }

    /// <summary>
    /// 시간 초기화
    /// </summary>
    public void ResetTime()
    {
        Pause();
        currentTime = 0f;
        OnTimeUpdated?.Invoke(isPlaying);
    }

    /// <summary>
    /// 특정 시간대로 점프
    /// </summary>
    public void JumpTo(float targetTime)
    {
        Pause();
        currentTime = Mathf.Clamp(targetTime, 0f, maxTime); 
        OnTimeUpdated?.Invoke(isPlaying); // 이벤트 현재시간으로 
    }

    /// <summary>
    /// 과거로 되감기 (안쓸거 같으면 삭제)
    /// </summary>
    public void RewindTo(float targetTime)
    {
        if (targetTime < currentTime)
        {
            Pause();
            currentTime = Mathf.Clamp(targetTime, 0f, maxTime);
            OnTimeUpdated?.Invoke(isPlaying);
        }
    }
}
