using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ���콺 ���¸� �����ϴ� �Ŵ��� Ŭ���� (MonoBehaviour)
/// </summary>
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// ���콺 Ŀ���� �����ϰ� ����ϴ� (���� ��� ��)
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// ���콺 Ŀ���� ���̰� �ϰ� ���� â ���� �����մϴ� (UI ���� �� �Ͻ����� ��)
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    /// <summary>
    /// ������ �����Ӱ� ���콺�� Ǯ�� ���� �� ��� (��: ���� â ��)
    /// </summary>
    public void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}