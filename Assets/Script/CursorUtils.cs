using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CursorUtils
{
    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // ���콺 Ŀ�� ����
        Cursor.visible = false;                   // ���콺 ����
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;   // ���콺 Ŀ�� ����
        Cursor.visible = true;                    // ���콺 ���̱�
    }
}

