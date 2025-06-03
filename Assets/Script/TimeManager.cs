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
    public float maxTime = 10f;         // ��ü Ÿ�Ӷ��� ����
    public float currentTime = 0f;      // ���� �ð� (0 ~ maxTime)
    public float playbackSpeed = 1f;    // ��� �ӵ� (1 = ������, -1 = ������)

    [Header("�ð� ����")]
    public bool isPlaying = false;

    // �ܺ� �ý����� �ð� ��ȭ�� �����ϵ��� �̺�Ʈ ����
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

        // �ð� ����
        currentTime += Time.deltaTime * playbackSpeed;

        // ���� ����
        currentTime = Mathf.Clamp(currentTime, 0f, maxTime);

        // �ð� ������Ʈ �̺�Ʈ ����
        OnTimeUpdated?.Invoke(currentTime);

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
    }

    /// <summary>
    /// �ð� ����
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// �ð� �ʱ�ȭ
    /// </summary>
    public void ResetTime()
    {
        Pause();
        currentTime = 0f;
        OnTimeUpdated?.Invoke(currentTime);
    }

    /// <summary>
    /// Ư�� �ð���� ����
    /// </summary>
    public void JumpTo(float targetTime)
    {
        Pause();
        currentTime = Mathf.Clamp(targetTime, 0f, maxTime);
        OnTimeUpdated?.Invoke(currentTime);
    }

    /// <summary>
    /// ���ŷ� �ǰ���
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
