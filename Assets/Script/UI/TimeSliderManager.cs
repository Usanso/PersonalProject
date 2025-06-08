using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 시간 슬라이더를 통해 게임 시간을 조작하는 UI 컨트롤러
/// TimeManager와 연동하여 시간 점프 및 시각적 표시 담당
/// 시간 맥스 실시간으로 변경_ 마지막시간을 최대시간으로
/// </summary>
public class TimeSliderManager : MonoBehaviour
{
    public static TimeSliderManager Instance { get; private set; }

    [Header("UI 요소")]
    [SerializeField] private Slider timeSlider; // 시간 조작용 슬라이더
    [SerializeField] private TextMeshProUGUI currentTimeText; // 현재 시간 표시 텍스트
    [SerializeField] private TextMeshProUGUI maxTimeText; // 최대 시간 표시 텍스트

    [Header("슬라이더 설정")]
    [SerializeField] private bool updateSliderInRealtime = true; // 실시간 슬라이더 업데이트 여부

    private bool isDragging = false; // 사용자가 슬라이더를 드래그 중인지 확인
    private TimeManager timeManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeSlider();
        SetupEventListeners();
    }

    private void Update()
    {
        // 실시간으로 슬라이더 업데이트 (드래그 중이 아닐 때만)
        if (updateSliderInRealtime && !isDragging)
        {
            UpdateSliderPosition();
        }

        UpdateTimeDisplay();
    }

    /// <summary>
    /// 슬라이더 초기 설정
    /// </summary>
    private void InitializeSlider()
    {
        timeManager = TimeManager.Instance;

        if (timeManager == null)
        {
            Debug.LogError("TimeManager를 찾을 수 없습니다!");
            return;
        }

        // 슬라이더 범위 설정 (0 ~ 최대 시간)
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = timeManager.maxTime;
            timeSlider.value = timeManager.currentTime;
        }
    }

    /// <summary>
    /// 슬라이더 이벤트 리스너 설정
    /// </summary>
    private void SetupEventListeners()
    {
        if (timeSlider != null)
        {
            // 슬라이더 값 변경 시 호출
            timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

            // 드래그 시작/종료 감지를 위한 이벤트 (선택사항)
            var eventTrigger = timeSlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = timeSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // 드래그 시작
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnSliderDragStart(); });
            eventTrigger.triggers.Add(pointerDown);

            // 드래그 종료
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnSliderDragEnd(); });
            eventTrigger.triggers.Add(pointerUp);
        }
    }

    /// <summary>
    /// 슬라이더 값이 변경될 때 호출되는 함수
    /// </summary>
    /// <param name="value">새로운 슬라이더 값</param>
    private void OnSliderValueChanged(float value)
    {
        // 사용자가 드래그 중일 때만 시간 점프 실행
        if (isDragging && timeManager != null)
        {
            timeManager.JumpToTime(value);
            ApplyRecordedStatesForAllRobots(value);
        }
    }

    /// <summary>
    /// 슬라이더 드래그 시작
    /// </summary>
    private void OnSliderDragStart()
    {
        isDragging = true;

        // 시간 일시정지 (드래그 중에는 시간이 흐르지 않도록)
        if (timeManager != null)
        {
            timeManager.Pause();
        }
    }

    /// <summary>
    /// 슬라이더 드래그 종료
    /// </summary>
    private void OnSliderDragEnd()
    {
        isDragging = false;

        // 드래그 종료 후 최종 시간 적용
        if (timeManager != null && timeSlider != null)
        {
            timeManager.JumpToTime(timeSlider.value);
            ApplyRecordedStatesForAllRobots(timeSlider.value);
        }
    }

    /// <summary>
    /// 현재 시간에 맞춰 슬라이더 위치 업데이트
    /// </summary>
    private void UpdateSliderPosition()
    {
        if (timeManager != null && timeSlider != null)
        {
            timeSlider.value = timeManager.currentTime;
        }
    }

    /// <summary>
    /// 시간 표시 텍스트 업데이트
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (timeManager == null) return;

        // 현재 시간 표시
        if (currentTimeText != null)
        {
            currentTimeText.text = $"현재: {timeManager.currentTime:F1}초";
        }

        // 최대 시간 표시
        if (maxTimeText != null)
        {
            maxTimeText.text = $"최대: {timeManager.maxTime:F1}초";
        }
    }

    /// <summary>
    /// 모든 로봇의 기록된 상태를 특정 시간대로 복원
    /// </summary>
    /// <param name="targetTime">복원할 시간</param>
    private void ApplyRecordedStatesForAllRobots(float targetTime)
    {
        if (RecordingManager.Instance == null) return;

        // 1. 먼저 아이템 상태 복원 (ItemManager 통해)
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.RestoreItemStatesAtTime(targetTime);
        }

        // 2. 로봇 상태 복원
        RobotController[] allRobots = FindObjectsOfType<RobotController>();

        foreach (RobotController robot in allRobots)
        {
            RobotState recordedState = RecordingManager.Instance.GetRobotState(robot.robotID, targetTime);

            if (recordedState != null)
            {
                // 위치와 회전 복원
                robot.transform.position = recordedState.position;
                robot.transform.rotation = recordedState.rotation;

                // 로봇의 들고 있는 아이템 참조 업데이트
                // (ItemManager에서 이미 아이템을 적절한 위치에 배치했으므로)
                robot.GetComponent<RobotController>()?.UpdateHeldItemReference();
            }
        }
    }

    /// <summary>
    /// 외부에서 슬라이더 최대값을 변경할 때 사용
    /// </summary>
    /// <param name="maxTime">새로운 최대 시간</param>
    public void SetMaxTime(float maxTime)
    {
        if (timeSlider != null)
        {
            timeSlider.maxValue = maxTime;
        }
    }

    /// <summary>
    /// 슬라이더 표시/숨김 토글
    /// </summary>
    /// <param name="visible">표시 여부</param>
    public void SetSliderVisible(bool visible)
    {
        if (timeSlider != null)
        {
            timeSlider.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// 컴포넌트 제거 시 이벤트 리스너 정리
    /// </summary>
    private void OnDestroy()
    {
        if (timeSlider != null)
        {
            timeSlider.onValueChanged.RemoveAllListeners();
        }
    }
}