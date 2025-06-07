using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

/// <summary>
/// ���� �� ���� �κ��� �����ϰ�, �÷��̾ Ư�� �κ��� ������ �� �ְ� ���ִ� ������ Ŭ����
/// </summary>
public class RobotSelectionManager : MonoBehaviour
{
    public static RobotSelectionManager Instance { get; private set; }

    [Header("�κ� ����")]
    public List<RobotController> robots = new List<RobotController>(); // ���� �� �����ϴ� ��� �κ� ����Ʈ
    public int selectedRobotIndex = 0; // ���� ���õ� �κ��� �ε���

    private RobotController currentRobot; // ���� ���õ� �κ�

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
    /// �κ� ��� �ʱ�ȭ �� �� �κ��� ���� ID �ο�
    /// </summary>
    private void InitializeRobots()
    {
        // �������� ��ϵ� �κ��� ���� ���, ������ �ڵ� �˻�
        if (robots.Count == 0)
        {
            RobotController[] foundRobots = FindObjectsOfType<RobotController>();
            robots.AddRange(foundRobots);
        }

        // �� �κ��� ���� ID �ο��ϰ� ��Ȱ��ȭ ���·� ����
        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].robotID = i;
            robots[i].SetActive(false);
        }
    }

    /// <summary>
    /// Ű���� ���� Ű (1~9) �Է¿� ���� �ش� �ε����� �κ��� ����
    /// </summary>
    private void HandleRobotSelection()
    {
        // ���� Ű�� ������ ��� 1~9
        for (int i = 0; i < robots.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectRobot(i); // �ش� �κ� ����
                break;
            }
        }
    }

    /// <summary>
    /// Ư�� �ε����� �κ��� �����Ͽ� Ȱ��ȭ�ϰ�, ���� �κ��� ��Ȱ��ȭ
    /// </summary>
    /// <param name="index">������ �κ��� �ε���</param>
    public void SelectRobot(int index)
    {
        if (index < 0 || index >= robots.Count) return; // ��ȿ ���� Ȯ��

        // ������ ���õ� �κ� ��Ȱ��ȭ
        if (currentRobot != null)
        {
            currentRobot.SetActive(false);
        }

        // �� �κ� ���� �� Ȱ��ȭ
        selectedRobotIndex = index;
        currentRobot = robots[selectedRobotIndex];
        currentRobot.SetActive(true);
        currentRobot.ResetMovementState();

        // ī�޶� �� �κ��� ����
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetTarget(currentRobot.transform);
        }
    }

    /// <summary>
    /// ���� ���õ� �κ� ��ü�� ��ȯ
    /// </summary>
    public RobotController GetSelectedRobot()
    {
        return currentRobot;
    }

    /// <summary>
    /// ���� ���õ� �κ��� ID(�ε���)�� ��ȯ
    /// </summary>
    public int GetSelectedRobotID()
    {
        return selectedRobotIndex;
    }

    /// <summary>
    /// �� �κ��� ��Ͽ� �߰��ϰ� ��Ȱ��ȭ ���·� ����
    /// </summary>
    /// <param name="robot">�߰��� �κ�</param>
    public void AddRobot(RobotController robot)
    {
        if (!robots.Contains(robot))
        {
            robot.robotID = robots.Count; // ID�� ���� ���� �������� �ο�
            robots.Add(robot);
            robot.SetActive(false);  // �⺻������ ��Ȱ��ȭ
        }
    }

    /// <summary>
    /// Ư�� �κ��� ��Ͽ��� �����ϰ�, ID ������
    /// </summary>
    /// <param name="robot">������ �κ�</param>
    public void RemoveRobot(RobotController robot)
    {
        if (robots.Contains(robot))
        {
            robots.Remove(robot);

            // ���� �ִ� �κ��鿡 ID �ٽ� �Ҵ�
            for (int i = 0; i < robots.Count; i++)
            {
                robots[i].robotID = i;
            }

            // ������ �κ��� ���� ���õ� �κ��̾��ٸ� ù ��° �κ� ����
            if (currentRobot == robot)
            {
                SelectRobot(0);
            }
        }
    }
}