using UnityEngine;
using System;

/// <summary>
/// ����: ���� �ð� �帧 ����, ���/�Ͻ�����/���� ���_
/// ���� ����: RobotController���� ������ ����, RecordingManager�� ��� ��� ����, UIManager�� �ð��� ������Ʈ_
/// �ֿ� ���: �ð� �ӵ� ����, �ð� ����, �ִ� �ð� ���� ����)_
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("�ð� ����")]
    [SerializeField] public float maxTime = 60f; // ��ü Ÿ�Ӷ��� ����
    [SerializeField] public float currentTime = 0f; // ���� �ð� (0 ~ maxTime) 
    public float playbackSpeed = 1f; // ��� �ӵ� (1 = ������, -1 = ������)
    public bool isPlaying = true;

    // �ܺ� �ý����� �ð� ��ȭ�� �����ϵ��� �̺�Ʈ ����
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
    /// �Ͻ����� ����Ű �����̽��ٸ� �����ϴ� �Լ�
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

    /// <summary>
    /// ���� �ð��� ������Ʈ �ϴ� �Լ�
    /// </summary>
    private void UpdateTime()
    {
        currentTime += Time.deltaTime * playbackSpeed;
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);
    }

    /// <summary>
    /// �ð��� 0�̸� �����ų�, ������ �ð��� �ʰ��� ��� �ڵ����� �Ͻ�����
    /// </summary>
    private void CheckTimeLimits()
    {
        if (currentTime >= maxTime || currentTime <= 0f)
        {
            Pause();
        }
    }

    /// <summary>
    /// �ð��� �����ϴ� �̺�Ʈ �˶��� ���� �Լ�
    /// </summary>
    private void TriggerEvents()
    {
        // ����ð� �˶�
        OnTimeChanged?.Invoke(currentTime);
    }

    /// <summary>
    /// �Ͻ����� ���¸� ���, ��� ���¸� �Ͻ����� �ϴ� ���
    /// </summary>
    public void TogglePlayPause()
    {
        if (isPlaying)
            Pause();
        else
            Play();
    }

    /// <summary>
    /// �ð� ��� ����
    /// </summary>
    public void Play(float speed = 1f)
    {
        playbackSpeed = speed;
        isPlaying = true;
        Time.timeScale = 1f; // ���� �ð� �帧
        OnTimeUpdated?.Invoke(isPlaying);
        MouseManager.Instance.LockCursor();
    }

    /// <summary>
    /// �ð� ����
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
        Time.timeScale = 0f; // Unity ��ü �ð� ����
        OnTimeUpdated?.Invoke(isPlaying);
        MouseManager.Instance.UnlockCursor();

    }

    /// <summary>
    /// �ð� �ʱ�ȭ
    /// </summary>
    public void ResetTime()
    {
        Pause();
        currentTime = 0f;
    }

    /// <summary>
    /// Ư�� �ð���� ����
    /// </summary>
    /// <param name="targetTime">������ �ð��� �����ϴ� ����</param>
    public void JumpToTime(float targetTime)
    {
        Pause();
        currentTime = Mathf.Clamp(targetTime, 0f, maxTime);
    }
}
