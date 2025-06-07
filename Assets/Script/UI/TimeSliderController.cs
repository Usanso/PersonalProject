using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TimeSliderController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI ��ҵ�")]
    public Slider timeSlider; // �ð� �����̴�
    public TextMeshProUGUI timeText; // �ð� ǥ�� �ؽ�Ʈ

    [Header("����� ����")]
    [SerializeField] private float currentTime = 0f; // ���� �ð� (����׿�, �б� ����)
    [SerializeField] private bool isUserDragging = false; // ����ڰ� �巡�� ������ Ȯ��

    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitializeSlider());
    }

    /// <summary>
    /// TimeManager �ν��Ͻ��� �غ�� ������ ��ٸ� �� �ʱ�ȭ
    /// </summary>
    private IEnumerator InitializeSlider()
    {
        // TimeManager �ν��Ͻ��� �غ�� ������ ���
        while (TimeManager.Instance == null)
        {
            yield return null;
        }

        // �����̴� �ʱ� ����
        timeSlider.minValue = 0f;
        timeSlider.maxValue = TimeManager.Instance.maxTime;
        timeSlider.value = 0f;

        // �����̴� �� ���� �̺�Ʈ ����
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // TimeManager �̺�Ʈ ����
        TimeManager.Instance.OnTimeChanged += OnTimeChanged;

        // �ʱ� �ð� ����
        currentTime = TimeManager.Instance.currentTime;
        UpdateTimeText();

        isInitialized = true;

        Debug.Log($"TimeSliderController �ʱ�ȭ �Ϸ� - MaxTime: {TimeManager.Instance.maxTime}");
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= OnTimeChanged;
        }

        // �����̴� �̺�Ʈ ����
        if (timeSlider != null)
        {
            timeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // ����ڰ� �巡�� ���� �ƴ� ���� �����̴� �� ������Ʈ
        if (!isUserDragging)
        {
            timeSlider.value = currentTime;
        }

        UpdateTimeText();
    }

    /// <summary>
    /// TimeManager���� �ð� ���� �̺�Ʈ�� �޴� �Լ�
    /// </summary>
    /// <param name="newTime">���ο� �ð� ��</param>
    public void OnTimeChanged(float newTime)
    {
        currentTime = newTime;
    }

    /// <summary>
    /// �����̴� ���� ����� �� ȣ��Ǵ� �Լ�
    /// </summary>
    /// <param name="value">�����̴��� ���ο� ��</param>
    public void OnSliderValueChanged(float value)
    {
        if (!isInitialized) return;

        // ����ڰ� ���� �����̴��� ������ ��쿡�� �ð� ���� ����
        if (isUserDragging)
        {
            TimeManager.Instance.JumpToTime(value);
            currentTime = value;
            Debug.Log($"�����̴��� �ð� ����: {value:F1}��");
        }
    }

    /// <summary>
    /// ���콺 �����Ͱ� �����̴��� ���� �� ȣ��
    /// </summary>
    /// <param name="eventData">������ �̺�Ʈ ������</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        isUserDragging = true;
        Debug.Log("�����̴� �巡�� ����");
    }

    /// <summary>
    /// ���콺 �����͸� �� �� ȣ��
    /// </summary>
    /// <param name="eventData">������ �̺�Ʈ ������</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        isUserDragging = false;
        Debug.Log("�����̴� �巡�� ����");
    }

    /// <summary>
    /// �ð��� Ư�� ������ �����ϴ� �Լ� (�ܺ� ȣ���)
    /// </summary>
    /// <param name="time">������ �ð�</param>
    public void SetTime(float time)
    {
        if (!isInitialized) return;

        float clampedTime = Mathf.Clamp(time, 0f, TimeManager.Instance.maxTime);
        TimeManager.Instance.JumpToTime(clampedTime);
        currentTime = clampedTime;
        timeSlider.value = clampedTime;
        UpdateTimeText();

        Debug.Log($"�ܺο��� �ð� ����: {clampedTime:F1}��");
    }

    /// <summary>
    /// �ð� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    /// </summary>
    void UpdateTimeText()
    {
        if (timeText != null)
        {
            // ��:�� �������� ǥ�� (��: 01:30)
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// �ִ� �ð��� ����Ǿ��� �� �����̴� �ִ밪 ������Ʈ
    /// </summary>
    public void UpdateMaxTime()
    {
        if (isInitialized && TimeManager.Instance != null)
        {
            timeSlider.maxValue = TimeManager.Instance.maxTime;
            Debug.Log($"�����̴� �ִ밪 ������Ʈ: {TimeManager.Instance.maxTime}��");
        }
    }
}