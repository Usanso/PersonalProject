using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public WheelSensor leftSensor;
    public WheelSensor rightSensor;

    public float moveSpeed = 5f;
    public float rotationSpeed = 300f; // ȸ�� �ӵ� ����
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

        // ������Ʈ ���� �������� ��ȯ
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;

        inputDirection = (right * h + forward * v).normalized;

        // ȸ�� �Է� �ʱ�ȭ
        rotateInput = 0f;

        if (Input.GetKey(KeyCode.Q)) rotateInput = -1f;
        else if (Input.GetKey(KeyCode.E)) rotateInput = 1f;
    }

    private void HandlePosition()
    {
        Vector3 desiredVelocity = inputDirection * moveSpeed;
        Vector3 currentVelocity = rb.velocity;
        Vector3 neededVelocityChange = desiredVelocity - currentVelocity;

        // ������Ʈ ���� ������ XZ ��鸸 ���
        neededVelocityChange.y = 0f;

        // �� ���� (�ӵ����� ���) + ����
        Vector3 force = neededVelocityChange * rb.mass / Time.fixedDeltaTime;

        // �������� �� �й�
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
            direction += rotateInput; // ���� ���� ȸ�� ����

        if (rightSensor.isGrounded)
            direction += rotateInput; // ������ ���� ȸ�� ����

        // ���� ȸ�� ���� (Y�� ȸ�� torque ���)
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

        // ���� ����Ű, �Ʒ��� �����Է� Ű

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
