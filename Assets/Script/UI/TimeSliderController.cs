using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TimeSliderController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI 요소들")]
    public Slider timeSlider; // 시간 슬라이더
    public TextMeshProUGUI timeText; // 시간 표시 텍스트

    [Header("디버그 정보")]
    [SerializeField] private float currentTime = 0f; // 현재 시간 (디버그용, 읽기 전용)
    [SerializeField] private bool isUserDragging = false; // 사용자가 드래그 중인지 확인

    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitializeSlider());
    }

    /// <summary>
    /// TimeManager 인스턴스가 준비될 때까지 기다린 후 초기화
    /// </summary>
    private IEnumerator InitializeSlider()
    {
        // TimeManager 인스턴스가 준비될 때까지 대기
        while (TimeManager.Instance == null)
        {
            yield return null;
        }

        // 슬라이더 초기 설정
        timeSlider.minValue = 0f;
        timeSlider.maxValue = TimeManager.Instance.maxTime;
        timeSlider.value = 0f;

        // 슬라이더 값 변경 이벤트 연결
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // TimeManager 이벤트 구독
        TimeManager.Instance.OnTimeChanged += OnTimeChanged;

        // 초기 시간 설정
        currentTime = TimeManager.Instance.currentTime;
        UpdateTimeText();

        isInitialized = true;

        Debug.Log($"TimeSliderController 초기화 완료 - MaxTime: {TimeManager.Instance.maxTime}");
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= OnTimeChanged;
        }

        // 슬라이더 이벤트 해제
        if (timeSlider != null)
        {
            timeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // 사용자가 드래그 중이 아닐 때만 슬라이더 값 업데이트
        if (!isUserDragging)
        {
            timeSlider.value = currentTime;
        }

        UpdateTimeText();
    }

    /// <summary>
    /// TimeManager에서 시간 변경 이벤트를 받는 함수
    /// </summary>
    /// <param name="newTime">새로운 시간 값</param>
    public void OnTimeChanged(float newTime)
    {
        currentTime = newTime;
    }

    /// <summary>
    /// 슬라이더 값이 변경될 때 호출되는 함수
    /// </summary>
    /// <param name="value">슬라이더의 새로운 값</param>
    public void OnSliderValueChanged(float value)
    {
        if (!isInitialized) return;

        // 사용자가 직접 슬라이더를 조작한 경우에만 시간 점프 실행
        if (isUserDragging)
        {
            TimeManager.Instance.JumpToTime(value);
            currentTime = value;
            Debug.Log($"슬라이더로 시간 변경: {value:F1}초");
        }
    }

    /// <summary>
    /// 마우스 포인터가 슬라이더를 누를 때 호출
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        isUserDragging = true;
        Debug.Log("슬라이더 드래그 시작");
    }

    /// <summary>
    /// 마우스 포인터를 뗄 때 호출
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        isUserDragging = false;
        Debug.Log("슬라이더 드래그 종료");
    }

    /// <summary>
    /// 시간을 특정 값으로 설정하는 함수 (외부 호출용)
    /// </summary>
    /// <param name="time">설정할 시간</param>
    public void SetTime(float time)
    {
        if (!isInitialized) return;

        float clampedTime = Mathf.Clamp(time, 0f, TimeManager.Instance.maxTime);
        TimeManager.Instance.JumpToTime(clampedTime);
        currentTime = clampedTime;
        timeSlider.value = clampedTime;
        UpdateTimeText();

        Debug.Log($"외부에서 시간 설정: {clampedTime:F1}초");
    }

    /// <summary>
    /// 시간 텍스트를 업데이트하는 함수
    /// </summary>
    void UpdateTimeText()
    {
        if (timeText != null)
        {
            // 분:초 형식으로 표시 (예: 01:30)
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// 최대 시간이 변경되었을 때 슬라이더 최대값 업데이트
    /// </summary>
    public void UpdateMaxTime()
    {
        if (isInitialized && TimeManager.Instance != null)
        {
            timeSlider.maxValue = TimeManager.Instance.maxTime;
            Debug.Log($"슬라이더 최대값 업데이트: {TimeManager.Instance.maxTime}초");
        }
    }
}