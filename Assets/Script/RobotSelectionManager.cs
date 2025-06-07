using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

/// <summary>
/// 게임 내 여러 로봇을 관리하고, 플레이어가 특정 로봇을 선택할 수 있게 해주는 관리자 클래스
/// </summary>
public class RobotSelectionManager : MonoBehaviour
{
    public static RobotSelectionManager Instance { get; private set; }

    [Header("로봇 관리")]
    public List<RobotController> robots = new List<RobotController>(); // 게임 내 존재하는 모든 로봇 리스트
    public int selectedRobotIndex = 0; // 현재 선택된 로봇의 인덱스

    private RobotController currentRobot; // 현재 선택된 로봇

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
        InitializeRobots();
        SelectRobot(0);
    }

    private void Update()
    {
        HandleRobotSelection();
    }

    /// <summary>
    /// 로봇 목록 초기화 및 각 로봇에 고유 ID 부여
    /// </summary>
    private void InitializeRobots()
    {
        // 수동으로 등록된 로봇이 없을 경우, 씬에서 자동 검색
        if (robots.Count == 0)
        {
            RobotController[] foundRobots = FindObjectsOfType<RobotController>();
            robots.AddRange(foundRobots);
        }

        // 각 로봇에 고유 ID 부여하고 비활성화 상태로 시작
        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].robotID = i;
            robots[i].SetActive(false);
        }
    }

    /// <summary>
    /// 키보드 숫자 키 (1~9) 입력에 따라 해당 인덱스의 로봇을 선택
    /// </summary>
    private void HandleRobotSelection()
    {
        // 숫자 키가 눌렸을 경우 1~9
        for (int i = 0; i < robots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectRobot(i); // 해당 로봇 선택
                break;
            }
        }
    }

    /// <summary>
    /// 특정 인덱스의 로봇을 선택하여 활성화하고, 이전 로봇은 비활성화
    /// </summary>
    /// <param name="index">선택할 로봇의 인덱스</param>
    public void SelectRobot(int index)
    {
        if (index < 0 || index >= robots.Count) return; // 유효 범위 확인

        // 이전에 선택된 로봇 비활성화
        if (currentRobot != null)
        {
            currentRobot.SetActive(false);
        }

        // 새 로봇 선택 및 활성화
        selectedRobotIndex = index;
        currentRobot = robots[selectedRobotIndex];
        currentRobot.SetActive(true);
        currentRobot.ResetMovementState();

        // 카메라를 새 로봇에 맞춤
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetTarget(currentRobot.transform);
        }
    }

    /// <summary>
    /// 현재 선택된 로봇 객체를 반환
    /// </summary>
    public RobotController GetSelectedRobot()
    {
        return currentRobot;
    }

    /// <summary>
    /// 현재 선택된 로봇의 ID(인덱스)를 반환
    /// </summary>
    public int GetSelectedRobotID()
    {
        return selectedRobotIndex;
    }

    /// <summary>
    /// 새 로봇을 목록에 추가하고 비활성화 상태로 설정
    /// </summary>
    /// <param name="robot">추가할 로봇</param>
    public void AddRobot(RobotController robot)
    {
        if (!robots.Contains(robot))
        {
            robot.robotID = robots.Count; // ID는 현재 개수 기준으로 부여
            robots.Add(robot);
            robot.SetActive(false);  // 기본적으로 비활성화
        }
    }

    /// <summary>
    /// 특정 로봇을 목록에서 제거하고, ID 재정렬
    /// </summary>
    /// <param name="robot">제거할 로봇</param>
    public void RemoveRobot(RobotController robot)
    {
        if (robots.Contains(robot))
        {
            robots.Remove(robot);

            // 남아 있는 로봇들에 ID 다시 할당
            for (int i = 0; i < robots.Count; i++)
            {
                robots[i].robotID = i;
            }

            // 제거한 로봇이 현재 선택된 로봇이었다면 첫 번째 로봇 선택
            if (currentRobot == robot)
            {
                SelectRobot(0);
            }
        }
    }
}