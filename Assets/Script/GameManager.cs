using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ���� ���¸� �̸����� ����
/// </summary>
public enum GameState
{
    Waiting,   // ���� ���� �� (��� ����)
    Playing,   // ���� ���� ��
    Victory,   // �÷��̾ �̱� ����
    Defeat     // �÷��̾ �� ����
}

/// <summary>
/// ����: ���� ��ü ���� ����, �������� ����, �¸� ���� üũ_
/// ���� ����: ��� �Ŵ������� �Ѱ��ϸ�, StageManager�� UIManager���� ���� ���� ��ȭ�� �˸�_
/// �ֿ� ���: ���� ����/����, �������� Ŭ���� üũ, ��ü ���� ���� ����_
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Waiting; // ���� ������ ����

    // [Header("���� �Ŵ���")]
    // public StageManager stageManager;
    // public UIManager uiManager;

    private void Awake()
    {
        // �̱��� ���� GameManager�� �̹� �ϳ� �ִٸ�, ���� ���� �� ���ֱ�
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this; // GameManager �ϳ����� �ν��Ͻ�

        DontDestroyOnLoad(gameObject); // �� ������Ʈ�� ���� �ٲ� ����
    }

    private void Update()
    {
        // ���� ���°� ���� ���� ���� �¸� ������ ��� üũ
        if (CurrentState == GameState.Playing)
        {
            CheckVictoryCondition();
        }
    }

    /// <summary>
    /// ������ ������ �� ȣ���ϴ� �Լ�
    /// </summary>
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        // uiManager?.OnGameStarted();     // UI�� �˸���
        // �� ����ǥ�� �Ʒ��� �������
        // if (uiManager != null)
        // {
        //     uiManager.OnGameEnded();
        // }

        // stageManager?.OnGameStarted();  // �������� ���� ó��
        Debug.Log("���� ����!");
    }

    /// <summary>
    /// ���ӿ��� �̰��� �� �����ϴ� �Լ�
    /// </summary>
    public void SetVictory()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Victory;
        // uiManager?.OnGameEnded(true);   // UI�� �¸� ����
        Invoke(nameof(LoadNextStage), 2f); // 2�� �� ���� �������� �ε�
        Debug.Log("�¸�!");
    }

    /// <summary>
    /// ���ӿ��� ���� �� �����ϴ� �Լ�
    /// </summary>
    public void SetDefeat()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Defeat;
        // uiManager?.OnGameEnded(false);  // UI�� �й� ����
        Invoke(nameof(ReloadStage), 2f); // 2�� �� �ٽ� ����
        Debug.Log("�й�!");
    }

    /// <summary>
    /// �¸� ������ Ȯ���ϴ� �Լ�
    /// </summary>
    private void CheckVictoryCondition()
    {
        // �������� ���� ���ڸ��� ������, �ڸ��� ���� ������ => ������ ���������� �ױ� ����
        if (GameObject.FindGameObjectsWithTag("Item").Length == 0)
        {
            SetVictory();
        }
    }

    /// <summary>
    /// ���� ������ �̵��ϴ� �Լ�
    /// </summary>
    public void LoadNextStage()
    {
        // ���� ���� ������ �����ͼ� +1 �� ���� ������ �̵�
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// ���� ���� �ٽ� �ҷ����� �Լ� (�絵��)
    /// </summary>
    public void ReloadStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}