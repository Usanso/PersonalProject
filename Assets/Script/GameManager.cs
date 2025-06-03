using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 상태를 이름으로 구분
/// </summary>
public enum GameState
{
    Waiting,   // 게임 시작 전 (대기 상태)
    Playing,   // 게임 진행 중
    Victory,   // 플레이어가 이긴 상태
    Defeat     // 플레이어가 진 상태
}

/// <summary>
/// 역할: 게임 전체 상태 관리, 스테이지 진행, 승리 조건 체크_
/// 영향 관계: 모든 매니저들을 총괄하며, StageManager와 UIManager에게 게임 상태 변화를 알림_
/// 주요 기능: 게임 시작/종료, 스테이지 클리어 체크, 전체 게임 루프 관리_
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Waiting; // 현재 게임의 상태

    // [Header("협력 매니저")]
    // public StageManager stageManager;
    // public UIManager uiManager;

    private void Awake()
    {
        // 싱글톤 세팅 GameManager가 이미 하나 있다면, 지금 생긴 건 없애기
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this; // GameManager 하나뿐인 인스턴스

        DontDestroyOnLoad(gameObject); // 이 오브젝트는 씬이 바뀌어도 유지
    }

    private void Update()
    {
        // 현재 상태가 게임 중일 때만 승리 조건을 계속 체크
        if (CurrentState == GameState.Playing)
        {
            CheckVictoryCondition();
        }
    }

    /// <summary>
    /// 게임을 시작할 때 호출하는 함수
    /// </summary>
    public void StartGame()
    {
        CurrentState = GameState.Playing;
        // uiManager?.OnGameStarted();     // UI에 알리기
        // 위 물음표는 아래의 축약형임
        // if (uiManager != null)
        // {
        //     uiManager.OnGameEnded();
        // }

        // stageManager?.OnGameStarted();  // 스테이지 시작 처리
        Debug.Log("게임 시작!");
    }

    /// <summary>
    /// 게임에서 이겼을 때 실행하는 함수
    /// </summary>
    public void SetVictory()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Victory;
        // uiManager?.OnGameEnded(true);   // UI에 승리 전달
        Invoke(nameof(LoadNextStage), 2f); // 2초 후 다음 스테이지 로드
        Debug.Log("승리!");
    }

    /// <summary>
    /// 게임에서 졌을 때 실행하는 함수
    /// </summary>
    public void SetDefeat()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Defeat;
        // uiManager?.OnGameEnded(false);  // UI에 패배 전달
        Invoke(nameof(ReloadStage), 2f); // 2초 후 다시 시작
        Debug.Log("패배!");
    }

    /// <summary>
    /// 승리 조건을 확인하는 함수
    /// </summary>
    private void CheckVictoryCondition()
    {
        // 아이템이 전부 제자리에 들어갔을때, 자리에 들어가면 아이템 => 정리된 아이템으로 테그 변경
        if (GameObject.FindGameObjectsWithTag("Item").Length == 0)
        {
            SetVictory();
        }
    }

    /// <summary>
    /// 다음 씬으로 이동하는 함수
    /// </summary>
    public void LoadNextStage()
    {
        // 현재 씬의 순번을 가져와서 +1 → 다음 씬으로 이동
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// 현재 씬을 다시 불러오는 함수 (재도전)
    /// </summary>
    public void ReloadStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}