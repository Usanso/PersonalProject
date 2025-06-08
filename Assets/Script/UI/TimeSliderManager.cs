using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �ð� �����̴��� ���� ���� �ð��� �����ϴ� UI ��Ʈ�ѷ�
/// TimeManager�� �����Ͽ� �ð� ���� �� �ð��� ǥ�� ���
/// �ð� �ƽ� �ǽð����� ����_ �������ð��� �ִ�ð�����
/// </summary>
public class TimeSliderManager : MonoBehaviour
{
    public static TimeSliderManager Instance { get; private set; }

    [Header("UI ���")]
    [SerializeField] private Slider timeSlider; // �ð� ���ۿ� �����̴�
    [SerializeField] private TextMeshProUGUI currentTimeText; // ���� �ð� ǥ�� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI maxTimeText; // �ִ� �ð� ǥ�� �ؽ�Ʈ

    [Header("�����̴� ����")]
    [SerializeField] private bool updateSliderInRealtime = true; // �ǽð� �����̴� ������Ʈ ����

    private bool isDragging = false; // ����ڰ� �����̴��� �巡�� ������ Ȯ��
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
        // �ǽð����� �����̴� ������Ʈ (�巡�� ���� �ƴ� ����)
        if (updateSliderInRealtime && !isDragging)
        {
            UpdateSliderPosition();
        }

        UpdateTimeDisplay();
    }

    /// <summary>
    /// �����̴� �ʱ� ����
    /// </summary>
    private void InitializeSlider()
    {
        timeManager = TimeManager.Instance;

        if (timeManager == null)
        {
            Debug.LogError("TimeManager�� ã�� �� �����ϴ�!");
            return;
        }

        // �����̴� ���� ���� (0 ~ �ִ� �ð�)
        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = timeManager.maxTime;
            timeSlider.value = timeManager.currentTime;
        }
    }

    /// <summary>
    /// �����̴� �̺�Ʈ ������ ����
    /// </summary>
    private void SetupEventListeners()
    {
        if (timeSlider != null)
        {
            // �����̴� �� ���� �� ȣ��
            timeSlider.onValueChanged.AddListener(OnSliderValueChanged);

            // �巡�� ����/���� ������ ���� �̺�Ʈ (���û���)
            var eventTrigger = timeSlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = timeSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // �巡�� ����
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnSliderDragStart(); });
            eventTrigger.triggers.Add(pointerDown);

            // �巡�� ����
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnSliderDragEnd(); });
            eventTrigger.triggers.Add(pointerUp);
        }
    }

    /// <summary>
    /// �����̴� ���� ����� �� ȣ��Ǵ� �Լ�
    /// </summary>
    /// <param name="value">���ο� �����̴� ��</param>
    private void OnSliderValueChanged(float value)
    {
        // ����ڰ� �巡�� ���� ���� �ð� ���� ����
        if (isDragging && timeManager != null)
        {
            timeManager.JumpToTime(value);
            ApplyRecordedStatesForAllRobots(value);
        }
    }

    /// <summary>
    /// �����̴� �巡�� ����
    /// </summary>
    private void OnSliderDragStart()
    {
        isDragging = true;

        // �ð� �Ͻ����� (�巡�� �߿��� �ð��� �帣�� �ʵ���)
        if (timeManager != null)
        {
            timeManager.Pause();
        }
    }

    /// <summary>
    /// �����̴� �巡�� ����
    /// </summary>
    private void OnSliderDragEnd()
    {
        isDragging = false;

        // �巡�� ���� �� ���� �ð� ����
        if (timeManager != null && timeSlider != null)
        {
            timeManager.JumpToTime(timeSlider.value);
            ApplyRecordedStatesForAllRobots(timeSlider.value);
        }
    }

    /// <summary>
    /// ���� �ð��� ���� �����̴� ��ġ ������Ʈ
    /// </summary>
    private void UpdateSliderPosition()
    {
        if (timeManager != null && timeSlider != null)
        {
            timeSlider.value = timeManager.currentTime;
        }
    }

    /// <summary>
    /// �ð� ǥ�� �ؽ�Ʈ ������Ʈ
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (timeManager == null) return;

        // ���� �ð� ǥ��
        if (currentTimeText != null)
        {
            currentTimeText.text = $"����: {timeManager.currentTime:F1}��";
        }

        // �ִ� �ð� ǥ��
        if (maxTimeText != null)
        {
            maxTimeText.text = $"�ִ�: {timeManager.maxTime:F1}��";
        }
    }

    /// <summary>
    /// ��� �κ��� ��ϵ� ���¸� Ư�� �ð���� ����
    /// </summary>
    /// <param name="targetTime">������ �ð�</param>
    private void ApplyRecordedStatesForAllRobots(float targetTime)
    {
        if (RecordingManager.Instance == null) return;

        // 1. ���� ������ ���� ���� (ItemManager ����)
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.RestoreItemStatesAtTime(targetTime);
        }

        // 2. �κ� ���� ����
        RobotController[] allRobots = FindObjectsOfType<RobotController>();

        foreach (RobotController robot in allRobots)
        {
            RobotState recordedState = RecordingManager.Instance.GetRobotState(robot.robotID, targetTime);

            if (recordedState != null)
            {
                // ��ġ�� ȸ�� ����
                robot.transform.position = recordedState.position;
                robot.transform.rotation = recordedState.rotation;

                // �κ��� ��� �ִ� ������ ���� ������Ʈ
                // (ItemManager���� �̹� �������� ������ ��ġ�� ��ġ�����Ƿ�)
                robot.GetComponent<RobotController>()?.UpdateHeldItemReference();
            }
        }
    }

    /// <summary>
    /// �ܺο��� �����̴� �ִ밪�� ������ �� ���
    /// </summary>
    /// <param name="maxTime">���ο� �ִ� �ð�</param>
    public void SetMaxTime(float maxTime)
    {
        if (timeSlider != null)
        {
            timeSlider.maxValue = maxTime;
        }
    }

    /// <summary>
    /// �����̴� ǥ��/���� ���
    /// </summary>
    /// <param name="visible">ǥ�� ����</param>
    public void SetSliderVisible(bool visible)
    {
        if (timeSlider != null)
        {
            timeSlider.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// ������Ʈ ���� �� �̺�Ʈ ������ ����
    /// </summary>
    private void OnDestroy()
    {
        if (timeSlider != null)
        {
            timeSlider.onValueChanged.RemoveAllListeners();
        }
    }
}