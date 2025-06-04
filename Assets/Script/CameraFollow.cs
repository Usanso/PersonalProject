using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 로봇
    public Vector3 offset = new Vector3(0, 5, -8);

    public static CameraFollow Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        if (!target) return; 

        // 오브젝트가 회전을 하면 카메라가 그 앞을 찍는 방식
        // 추후에 반대로 수정, 카메라가 움직이고 오브젝트가 그것을 따라가도록

        Quaternion yawRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
        Vector3 desiredPosition = target.position + yawRotation * offset;

        // 즉시 위치 설정 (Lerp 제거)
        transform.position = desiredPosition;

        // 즉시 방향 설정
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
