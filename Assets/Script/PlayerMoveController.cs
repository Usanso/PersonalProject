using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 로봇의 특정 시점 행동 데이터를 담는 구조체
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMoveController : MonoBehaviour
{
    // 컴포넌트 참고
    private Rigidbody rb;
    public Transform cameraTransform;
    private RobotActionRecorder actionRecorder;

    [Header("이동 조작 설정")]
    private bool isGrounded = false;
    private Vector3 inputDirection;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] public float mouseSensitivity = 5f;

    // 가속도 옵션
    private float acceleration = 1f; // 가속도 (높을수록 빠르게 목표 속도에 도달)
    private float damping = 0.5f;       // 감속도 (빠르게 멈추는 정도)
    private Vector3 smoothedVelocity = Vector3.zero; 

    // [Header("상태 변수")]
    // [SerializeField] private bool isPlayerControlled = false; 플레이어가 조작 중인지
    // private GameObject carriedItem = null; 현재 들고 있는 물건, 무게에 따라 가속도 감속도 변경 (후추)

    #region 초기화
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // actionRecorder = GetComponent<RobotActionRecorder>();
    }

    void Start()
    {
        // 자신을 카메라 타겟으로 설정 (추후 변경 필요)
        CameraFollow.Instance?.SetTarget(transform); 
    }

    #endregion

    private void OnCollisionStay (Collision collision)
    {
        if (isGrounded == true) return;
        if (collision.collider.CompareTag("Ground")) isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground")) isGrounded = false;
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
        transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0); // Y축 회전
    }

    private void HandlePosition()
    {
        Vector3 desiredVelocity = inputDirection * moveSpeed;

        // 현재 속도와 목표 속도 사이 보간 (Lerp or custom damp)
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

        // 감속이 필요한 상황: 입력이 거의 없을 때
        if (inputDirection.magnitude < 0.1f)
        {
            smoothedVelocity = Vector3.Lerp(smoothedVelocity, Vector3.zero, damping * Time.fixedDeltaTime);
        }

        Vector3 neededVelocityChange = smoothedVelocity - rb.velocity;
        neededVelocityChange.y = 0f;

        Vector3 force = neededVelocityChange * rb.mass / Time.fixedDeltaTime;

        if (isGrounded)
        {
            rb.AddForce(force, ForceMode.Force); 
        }
    }
}
