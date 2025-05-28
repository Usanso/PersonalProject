using UnityEngine;

public class WheelSensor : MonoBehaviour
{
    public bool isGrounded { get; private set; }

    private void OnTriggerStay(Collider other)
    {
        if (isGrounded) return;
        if (other.CompareTag("Ground")) isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground")) isGrounded = false;
    }
}

