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
    [SerializeField] public float maxTime = 10f;         // ��ü Ÿ�Ӷ��� ����
    [SerializeField] public float currentTime = 0f;      // ���� �ð� (0 ~ maxTime) 
    public float playbackSpeed = 1f;    // ��� �ӵ� (1 = ������, -1 = ������)

    [Header("�ð� ����")]
    public bool isPlaying = true;

    // �ܺ� �ý����� �ð� ��ȭ�� �����ϵ��� �̺�Ʈ ����
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

        // �ð� ����
        currentTime += Time.deltaTime * playbackSpeed;

        // ���� ����
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        // �ð� ������Ʈ �̺�Ʈ ����
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
    /// �ð��� 0�̸� �����ų�, ������ �ð��� �ʰ��� ��� �ڵ����� �Ͻ�����
    /// </summary>
    public void TimeMaxCheck()
    {
        // �ð��� ���� �����ϸ� ����
        if (currentTime >= maxTime || currentTime <= 0f)
        {
            Pause();
        }
    }

    /// <summary>
    /// �ð� ��� ����
    /// </summary>
    public void Play(float speed = 1f)
    {
        playbackSpeed = speed;
        isPlaying = true;
        Time.timeScale = 1f; // ���� �ð� �帧
        OnTimeUpdated?.Invoke(true);
        CursorUtils.LockCursor();
    }

    /// <summary>
    /// �ð� ����
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
        Time.timeScale = 0f; // Unity ��ü �ð� ����
        OnTimeUpdated?.Invoke(false);
        CursorUtils.UnlockCursor();

    }

    /// <summary>
    /// �ð� �ʱ�ȭ
    /// </summary>
    public void ResetTime()
    {
        Pause();
        currentTime = 0f;
        OnTimeUpdated?.Invoke(isPlaying);
    }

    /// <summary>
    /// Ư�� �ð���� ����
    /// </summary>
    public void JumpTo(float targetTime)
    {
        Pause();
        currentTime = Mathf.Clamp(targetTime, 0f, maxTime); 
        OnTimeUpdated?.Invoke(isPlaying); // �̺�Ʈ ����ð����� 
    }

    /// <summary>
    /// ���ŷ� �ǰ��� (�Ⱦ��� ������ ����)
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
