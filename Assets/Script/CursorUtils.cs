using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CursorUtils
{
    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 고정
        Cursor.visible = false;                   // 마우스 숨김
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;   // 마우스 커서 해제
        Cursor.visible = true;                    // 마우스 보이기
    }
}

