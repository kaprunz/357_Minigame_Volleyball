using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Player : MonoBehaviour
{
    public static Player instance;
    [SerializeField]
    private float forcePower;

    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    public int Points;

    private InputAction moveAction;
    private InputAction jumpAction;
    private Vector2 moveValue;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public enum PlayerState
    {
        Grounded,
        Jumping
    }
    public PlayerState currentState = PlayerState.Grounded;

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
        if (jumpAction.triggered&& currentState == PlayerState.Grounded)
        {
            rb.AddForce(Vector3.up * forcePower, ForceMode.Impulse);
            currentState = PlayerState.Jumping;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            currentState = PlayerState.Grounded;
        }
    }


}

