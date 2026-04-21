using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5.0f;
    public float gravity = -25.0f;
    public float jumpHeight = 2.0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Cameras")]
    public CinemachineCamera followCam;
    public CinemachineCamera securityCam;
    public CinemachineCamera zoomCam;

    [Header("Interaction")]
    public float interactionDistance = 4.0f;
    public LayerMask interactionLayer;
    private int anomaliesFound = 0;

    [Header("Events")]
    public UnityEvent onAnomalyFound;
    public UnityEvent onAllAnomaliesCleared;
    public UnityEvent onJumpStart;
    public UnityEvent onJumpLand;
    public UnityEvent onFixing;
    public UnityEvent onDance;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        SetCameraPriority(followCam);
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded && !wasGrounded && velocity.y < 0)
        {
            onJumpLand.Invoke();
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(vertical, 0f, -horizontal).normalized;

        if (direction.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            controller.Move(direction * walkSpeed * Time.deltaTime);
        }

        animator.SetFloat("Speed", direction.magnitude);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
            onJumpStart.Invoke();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        HandleCameraInput();
        HandleActionInput();

        wasGrounded = isGrounded;
    }

    private void HandleCameraInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetCameraPriority(followCam);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetCameraPriority(securityCam);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetCameraPriority(zoomCam);
    }

    private void SetCameraPriority(CinemachineCamera targetCam)
    {
        followCam.Priority = 10;
        securityCam.Priority = 10;
        zoomCam.Priority = 10;
        targetCam.Priority = 20;
    }

    private void HandleActionInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("InteractTrigger");
            HandleRaycast();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("FixingTrigger");
            onFixing.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            animator.SetTrigger("DanceTrigger");
            onDance.Invoke();
        }
    }

    private void HandleRaycast()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1.2f;

        if (Physics.Raycast(origin, transform.forward, out hit, interactionDistance, interactionLayer))
        {
            if (hit.collider.CompareTag("Anomaly"))
            {
                Destroy(hit.collider.gameObject);
                anomaliesFound++;
                onAnomalyFound.Invoke();

                if (anomaliesFound >= 3)
                {
                    onAllAnomaliesCleared.Invoke();
                }
            }
        }
    }
}