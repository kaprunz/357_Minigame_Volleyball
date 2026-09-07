using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float forcePower;

    [SerializeField]
    private Rigidbody rb;

    private InputAction moveAction;
    private InputAction jumpAction;
    private Vector2 moveValue;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Jump();
    }

    private void Movement()
    {
        moveValue = moveAction.ReadValue<Vector2>();
        rb.AddForce(moveValue.x * Vector3.right * forcePower);
        rb.AddForce(moveValue.y * Vector3.forward * forcePower);
    }
    
    private void Jump()
    {
        if (jumpAction.triggered)
        {
            rb.AddForce(Vector3.up * forcePower, ForceMode.Impulse);
        }
    }
}

