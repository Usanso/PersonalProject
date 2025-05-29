using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerHandle : MonoBehaviour
{
    public Transform cameraTransform;

    private bool isGrounded = false;
    
    [SerializeField] public float moveSpeed = 10f;

    private Rigidbody rb;
    private Vector3 inputDirection;

    [SerializeField] public float mouseSensitivity = 5f;
    private float mouseX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 자신을 카메라 타겟으로 설정 (추후 변경 필요)
        CameraFollow.Instance?.SetTarget(transform); 
    }
    
    private void OnCollisionStay (Collision collision)
    {
        if (isGrounded == true) return;
        if (collision.collider.CompareTag("Ground")) isGrounded = true;
        Debug.Log("붙음");
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground")) isGrounded = false;
        Debug.Log("떨어짐");
    }
    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        HandleMouseLook();
        HandleMovement();
    }
    private void HandleInput()
    {
        ArrowKeys();
    }

    private void HandleMovement()
    {
        // HandleRotation();
        HandlePosition();
    }

    private void ArrowKeys()
    {
        float horizontalh = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 forward = CameraFollow.Instance.GetViewForward();
        Vector3 right = CameraFollow.Instance.GetViewRight();

        inputDirection = (right * horizontalh + forward * vertical).normalized;
    }

    
    private void HandleMouseLook() // 마우스로 회전
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0); // Y축 회전
    }

    private void HandlePosition()
    {
        Vector3 desiredVelocity = inputDirection * moveSpeed;
        Vector3 currentVelocity = rb.velocity;
        Vector3 neededVelocityChange = desiredVelocity - currentVelocity;

        // 오브젝트 기준 방향의 XZ 평면만 고려
        neededVelocityChange.y = 0f;

        // 힘 강도 (속도차이 비례) + 제한
        Vector3 force = neededVelocityChange * rb.mass / Time.fixedDeltaTime;

        // 바퀴에만 힘 분배
        if (isGrounded)
        {
            rb.AddForceAtPosition(force * 0.5f, transform.position, ForceMode.Force);
        }

    }
}
