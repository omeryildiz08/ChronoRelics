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

    [Header("Background Follow")]
    [SerializeField] private Transform backgroundPlane;
    [SerializeField] private bool moveBackgroundWithCamera = true;

    [Tooltip("1 = kamera ile birebir hareket eder. 0.5 = parallax gibi daha yavaş hareket eder.")]
    [SerializeField] private float backgroundFollowMultiplier = 1f;

    [Tooltip("Background sadece X ekseninde kamerayı takip etsin mi?")]
    [SerializeField] private bool backgroundFollowX = true;

    [Tooltip("Background sadece Z ekseninde kamerayı takip etsin mi?")]
    [SerializeField] private bool backgroundFollowZ = true;

    private Vector3 targetPosition;
    private Vector3 velocity;

    private void Awake()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        Vector3 cameraPositionBeforeMove = transform.position;

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

        MoveBackgroundByCameraDelta(cameraPositionBeforeMove, transform.position);
    }

    private void MoveBackgroundByCameraDelta(Vector3 oldCameraPosition, Vector3 newCameraPosition)
    {
        if (!moveBackgroundWithCamera || backgroundPlane == null)
        {
            return;
        }

        Vector3 cameraDelta = newCameraPosition - oldCameraPosition;
        Vector3 backgroundDelta = Vector3.zero;

        if (backgroundFollowX)
        {
            backgroundDelta.x = cameraDelta.x * backgroundFollowMultiplier;
        }

        if (backgroundFollowZ)
        {
            backgroundDelta.z = cameraDelta.z * backgroundFollowMultiplier;
        }

        backgroundPlane.position += backgroundDelta;
    }
}