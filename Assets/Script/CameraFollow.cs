using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // ���� �κ�
    public Vector3 offset = new Vector3(0, 5, -8);

    public static CameraFollow Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (!target) return; 

        // ������Ʈ�� ȸ���� �ϸ� ī�޶� �� ���� ��� ���
        // ���Ŀ� �ݴ�� ����, ī�޶� �����̰� ������Ʈ�� �װ��� ���󰡵���

        Quaternion yawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        Vector3 desiredPosition = target.position + yawRotation * offset;

        // ��� ��ġ ���� (Lerp ����)
        transform.position = desiredPosition;

        // ��� ���� ����
        Vector3 lookTarget = target.position + Vector3.up * 1.5f;
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);
    }


    public Vector3 GetViewForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    public Vector3 GetViewRight()
    {
        Vector3 right = transform.right;
        right.y = 0f;
        return right.normalized;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
