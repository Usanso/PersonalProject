using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public WheelSensor leftSensor;
    public WheelSensor rightSensor;

    public float moveSpeed = 5f;
    public float rotationSpeed = 300f; // 회전 속도 조절
    private float rotateInput;

    private Rigidbody rb;
    private Vector3 inputDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // 오브젝트 기준 방향으로 변환
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;

        inputDirection = (right * h + forward * v).normalized;

        // 회전 입력 초기화
        rotateInput = 0f;

        if (Input.GetKey(KeyCode.Q)) rotateInput = -1f;
        else if (Input.GetKey(KeyCode.E)) rotateInput = 1f;
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
        if (leftSensor.isGrounded)
        {
            rb.AddForceAtPosition(force * 0.5f, leftSensor.transform.position, ForceMode.Force);
        }

        if (rightSensor.isGrounded)
        {
            rb.AddForceAtPosition(force * 0.5f, rightSensor.transform.position, ForceMode.Force);
        }
    }


    private void HandleRotation()
    {
        if (rotateInput == 0f) return;

        float direction = 0f;

        if (leftSensor.isGrounded)
            direction += rotateInput; // 왼쪽 바퀴 회전 방향

        if (rightSensor.isGrounded)
            direction += rotateInput; // 오른쪽 바퀴 회전 방향

        // 실제 회전 적용 (Y축 회전 torque 사용)
        if (direction != 0f)
        {
            Vector3 localTorque = transform.up * direction * rotationSpeed;
            rb.AddTorque(localTorque, ForceMode.Acceleration);
        }
    }

    private void HandleMovement()
    {
        HandleRotation();
        HandlePosition();

        // 위는 동시키, 아래는 개별입력 키

        //if (inputDirection != Vector3.zero)
        //{
        //    HandlePosition();
        //}
        //else if (rotateInput != 0f)
        //{
        //    HandleRotation();
        //}
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }
}
