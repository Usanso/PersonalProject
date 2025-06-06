using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 마우스 상태를 관리하는 매니저 클래스 (MonoBehaviour)
/// </summary>
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 마우스 커서를 고정하고 숨깁니다 (게임 재생 중)
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// 마우스 커서를 보이게 하고 게임 창 내에 고정합니다 (UI 조작 등 일시정지 중)
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    /// <summary>
    /// 완전히 자유롭게 마우스를 풀고 싶을 때 사용 (예: 설정 창 등)
    /// </summary>
    public void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}