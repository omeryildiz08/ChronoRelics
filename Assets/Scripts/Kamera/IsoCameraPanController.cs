using UnityEngine;

public class IsoCameraPanController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Bounds")]
    [SerializeField] private float minX = -25f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minZ = -25f;
    [SerializeField] private float maxZ = 10f;

    private Vector3 targetPosition;
    private Vector3 velocity;

    private void Awake()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D veya sol/sag ok
        float vertical = Input.GetAxisRaw("Vertical");     // W/S veya yukari/asagi ok

        Vector3 input = new Vector3(horizontal, 0f, vertical);

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        Vector3 cameraRight = transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 cameraForward = transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 moveDirection = cameraRight * input.x + cameraForward * input.z;

        targetPosition += moveDirection * moveSpeed * Time.deltaTime;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
        targetPosition.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}