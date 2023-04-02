using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveState : BaseState
{
    [SerializeField] Transform mainCamera;
    [SerializeField] Transform head;

    protected Rigidbody rb;
    protected float horizontalInput;
    protected float verticalInput;

    protected float upInput;
    
    protected bool isGrounded;

    protected static MovingPlatform platform;
    protected static Vector3 groundObjectVelocity;

    [SerializeField] float sensitivity;

    public override void OnAwake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStart()
    {
    }

    public override void OnEnter()
    {
        rb.useGravity = true;
        Physics.gravity = -transform.up * 9.81f;
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
    }

    public override void OnLateUpdate()
    {
        // replace w/ cam movement
        mainCamera.position = head.position;
        Vector3 headEuler = head.eulerAngles +
            new Vector3(-Input.GetAxis("Mouse Y") * sensitivity, 0, 0);
        if (headEuler.x > 180) headEuler.x -= 360;
        if (headEuler.x < 180) headEuler.x += 360;
        headEuler.x = Mathf.Clamp(headEuler.x, 270, 450);
        head.eulerAngles = headEuler;
        mainCamera.eulerAngles = head.eulerAngles;

        Vector3 bodyEuler = transform.eulerAngles +
            new Vector3(0, Input.GetAxis("Mouse X") * sensitivity, 0);
        if (bodyEuler.y > 180) headEuler.y -= 360;
        if (bodyEuler.y < 180) headEuler.y += 360;
        transform.eulerAngles = bodyEuler;

        //transform.rotation = Quaternion.LookRotation(transform.forward, -Physics.gravity);
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
            transform.localScale.y - sphereRadius + 0.1f))
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
}
