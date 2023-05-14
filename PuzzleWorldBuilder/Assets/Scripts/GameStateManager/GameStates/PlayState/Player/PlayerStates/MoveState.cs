using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveState : BaseState, IGravity
{
    [SerializeField] protected Transform cameraPivot;
    [SerializeField] protected Transform mainCamera;
    [SerializeField] protected Transform head;

    protected Rigidbody rb;
    protected float horizontalInput;
    protected float verticalInput;

    protected float upInput;

    protected bool isGrounded;

    protected static MovingPlatform platform;
    protected static Vector3 groundObjectVelocity;

    [SerializeField] float sensitivity;
    [SerializeField, Range(0, 0.2f)] float slerpSpeed;
    [SerializeField] LayerMask castLayers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEnter()
    {
        rb.isKinematic = false;
    }

    public override void OnExit()
    {
    }

    public override void OnFixedUpdate()
    {
    }

    public override void OnUpdate()
    {
        InputDetection();
        GroundDetection();

        if (rb.useGravity) RotateTowardsGravity(Physics.gravity);
    }

    public override void OnLateUpdate()
    {
        CameraMovement();
    }

    void InputDetection()
    {
        horizontalInput = 0;
        if (Input.GetKey(KeyCode.D)) horizontalInput += 1;
        if (Input.GetKey(KeyCode.A)) horizontalInput -= 1;
        
        verticalInput = 0;
        if (Input.GetKey(KeyCode.W)) verticalInput += 1;
        if (Input.GetKey(KeyCode.S)) verticalInput -= 1;
    }

    void GroundDetection()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        float sphereRadius = 0.9f;
        RaycastHit hit;
        if (Physics.SphereCast(ray,
            sphereRadius,
            out hit,
            transform.localScale.y - sphereRadius + 0.1f,
            castLayers))
        {
            if (hit.collider != null)
            {
                platform = hit.collider.GetComponent<MovingPlatform>();
            }
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            platform = null;
        }
    }

    void CameraMovement()
    {
        cameraPivot.position = head.position;
        cameraPivot.rotation = transform.rotation;

        Vector3 horizontalMouse = new Vector3(0, Input.GetAxis("Mouse X") * sensitivity, 0);
        Vector3 verticalMouse = new Vector3(-Input.GetAxis("Mouse Y") * sensitivity, 0);

        head.localEulerAngles += horizontalMouse;
        float xRot = mainCamera.localEulerAngles.x + verticalMouse.x;
        if (xRot > 180) xRot -= 360;
        if (xRot < 180) xRot += 360;
        xRot = Mathf.Clamp(xRot, 270, 450);
        mainCamera.localEulerAngles = new Vector3(xRot,
                                                  mainCamera.localEulerAngles.y + horizontalMouse.y,
                                                  mainCamera.localEulerAngles.z);
    }

    public void OnEnterZone()
    {
    }

    public void SetGravity(Vector3 direction)
    {
        rb.useGravity = false;
        rb.AddForce(direction);
        RotateTowardsGravity(direction);
    }

    public void OnExitZone()
    {
        rb.useGravity = true;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    void RotateTowardsGravity(Vector3 direction)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.FromToRotation(Vector3.down, direction),
            slerpSpeed);
    }
}
