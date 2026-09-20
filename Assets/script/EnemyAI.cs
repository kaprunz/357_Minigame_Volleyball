using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState
    {
        MimicPlayer,
        ChaseAndSpike
    }

    [Header("State Settings")]
    public AIState currentState = AIState.MimicPlayer;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform ballTransform;
    [SerializeField] private Rigidbody rb;

    [Header("Movement Tuning")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float positionTolerance = 0.15f;

    [Header("Jump & Spike Tuning")]
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float spikeDistance = 1.8f;
    [SerializeField] private float jumpHeightThreshold = 2.0f;
    [SerializeField] private float attackOffsetDistance = 0.6f;

    [Header("Overhead Detection Settings")]
    [SerializeField] private float overheadDistanceThreshold = 0.6f;  // XZ radius considered "overhead"
    [SerializeField] private float overheadMinHeight = 1.0f;           // Min ball height above head to trigger jump

    [Header("Jump Cooldown Settings")]
    [SerializeField] private float jumpCooldown = 1.5f;
    private float nextJumpTime = 0f;

    [SerializeField] private float netZPosition = 0f;

    public Player.PlayerState currentPhysicalState = Player.PlayerState.Grounded;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (ballTransform == null)
        {
            GameObject ballObj = GameObject.FindGameObjectWithTag("Ball");
            if (ballObj != null) ballTransform = ballObj.transform;
        }
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case AIState.MimicPlayer:
                HandleMimicState();
                break;

            case AIState.ChaseAndSpike:
                HandleChaseState();
                break;
        }
    }

    private void HandleMimicState()
    {
        if (playerTransform == null) return;

        float mirroredZ = netZPosition + (netZPosition - playerTransform.position.z);
        Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, mirroredZ);

        MoveTowardsTarget(targetPosition);
    }

    private void HandleChaseState()
    {
        if (ballTransform == null || playerTransform == null) return;

        // 1. Calculate direction vector from Ball to Player
        Vector3 ballToPlayer = (playerTransform.position - ballTransform.position);
        ballToPlayer.y = 0f;
        ballToPlayer.Normalize();

        // 2. Position AI BEHIND the ball relative to the player
        Vector3 desiredPosition = ballTransform.position - (ballToPlayer * attackOffsetDistance);
        desiredPosition.y = transform.position.y;

        // 3. Smoothly move toward positioning spot
        MoveTowardsTarget(desiredPosition);

        // 4. Horizontal 2D distance calculation
        Vector2 aiPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 ballPos2D = new Vector2(ballTransform.position.x, ballTransform.position.z);
        float horizontalDistance = Vector2.Distance(aiPos2D, ballPos2D);

        float heightDifference = ballTransform.position.y - transform.position.y;

        // 5. Jump Checks: Standard Spike Jump OR Emergency Overhead Jump
        bool isStandardSpike = horizontalDistance <= spikeDistance && heightDifference >= jumpHeightThreshold;
        bool isOverhead = horizontalDistance <= overheadDistanceThreshold && heightDifference >= overheadMinHeight;

        if ((isStandardSpike || isOverhead) &&
            currentPhysicalState == Player.PlayerState.Grounded &&
            Time.time >= nextJumpTime)
        {
            Jump();
        }
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector3 offset = targetPosition - transform.position;
        offset.y = 0f;

        if (offset.magnitude < positionTolerance)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.85f, rb.linearVelocity.y, rb.linearVelocity.z * 0.85f);
            return;
        }

        Vector3 targetVelocity = offset.normalized * moveSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;

        velocityChange = Vector3.ClampMagnitude(velocityChange, acceleration);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        currentPhysicalState = Player.PlayerState.Jumping;

        nextJumpTime = Time.time + jumpCooldown;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            currentPhysicalState = Player.PlayerState.Grounded;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            currentState = AIState.ChaseAndSpike;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            currentState = AIState.MimicPlayer;
        }
    }
}