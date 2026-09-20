using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [Header("Blob Shadow Settings")]
    public GameObject shadow;
    public float offset = 0.5f;
    public float maxShadowDistance = 25f;

    [Header("Bump Hit Settings (Grounded)")]
    [SerializeField] private float bumpUpForce = 8f;
    [SerializeField] private float bumpForwardForce = 4f;

    [Header("Spike Hit Settings (Jumping)")]
    [SerializeField] private float spikeUpForce = 2f;
    [SerializeField] private float spikeForwardForce = 15f;

    [Header("Wall Bounce Settings")]
    [SerializeField] private float wallUpwardBoost = 6f;      // Upward force added on wall impact
    [SerializeField] private float wallReflectionForce = 8f;   // Horizontal bounce power off walls

    [Header("Direction Settings")]
    [SerializeField, Range(0f, 1f)]
    private float directionalInfluence = 0.4f;

    [SerializeField, Range(0f, 60f)]
    private float maxSidewaysAngle = 45f;

    [SerializeField] private Rigidbody rb;

    // Internal Wall Bounce Cooldown
    private float wallBounceCooldown = 0.2f;
    private float nextWallBounceTime = 0f;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        UpdateBlobShadow();
    }

    private void UpdateBlobShadow()
    {
        if (shadow == null) return;

        Ray downRay = new Ray(transform.position - (Vector3.up * offset), Vector3.down);

        if (Physics.Raycast(downRay, out RaycastHit hit, maxShadowDistance))
        {
            if (!shadow.activeSelf) shadow.SetActive(true);
            shadow.transform.position = hit.point + (Vector3.up * 0.01f);
            shadow.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {
            if (shadow.activeSelf) shadow.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Check for Human Player Collision
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                HandleHit(collision.transform, Vector3.forward, player.currentState == Player.PlayerState.Jumping);
            }
        }
        // 2. Check for Enemy AI Collision
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                HandleHit(collision.transform, Vector3.back, enemy.currentPhysicalState == Player.PlayerState.Jumping);
            }
        }
        // 3. Wall Collision -> Adds upward pop with a 0.2s cooldown
        else if (collision.gameObject.CompareTag("Wall"))
        {
            if (Time.time >= nextWallBounceTime)
            {
                HandleWallBounce(collision);
            }
        }
        // 4. Floor Collision -> Triggers Point Scoring
        else if (collision.gameObject.CompareTag("Floor"))
        {
            HandlePointScored();
        }
    }

    private void HandleWallBounce(Collision collision)
    {
        // Set cooldown timestamp
        nextWallBounceTime = Time.time + wallBounceCooldown;

        // Get the impact contact normal pointing away from the wall
        ContactPoint contact = collision.contacts[0];
        Vector3 wallNormal = contact.normal;
        wallNormal.y = 0f; // Keep pure horizontal direction away from wall
        wallNormal.Normalize();

        // Reset linear velocity for predictable bounce trajectory
        rb.linearVelocity = Vector3.zero;

        // Combine outward reflection vector with upward boost
        Vector3 wallBounceForce = (wallNormal * wallReflectionForce) + (Vector3.up * wallUpwardBoost);

        rb.AddForce(wallBounceForce, ForceMode.Impulse);
    }

    private void HandlePointScored()
    {
        if (Referee.instance == null) return;

        // Immediately freeze the ball so it stays in place during the scoring pause
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Trigger the unified scoring routine on the Referee
        if (transform.position.z < 0f)
        {
            Referee.instance.ScorePoint("Enemy");
        }
        else
        {
            Referee.instance.ScorePoint("Player");
        }
    }

    private void HandleHit(Transform hitterTransform, Vector3 targetBaseDirection, bool isJumping)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 hitDirection = CalculateHitDirection(hitterTransform, targetBaseDirection);

        if (isJumping)
        {
            ApplyBallForce(hitDirection, spikeForwardForce, spikeUpForce);
        }
        else
        {
            ApplyBallForce(hitDirection, bumpForwardForce, bumpUpForce);
        }
    }

    private Vector3 CalculateHitDirection(Transform hitter, Vector3 baseDirection)
    {
        Vector3 awayFromHitter = (transform.position - hitter.position);
        awayFromHitter.y = 0f;

        if (awayFromHitter.sqrMagnitude < 0.001f)
        {
            awayFromHitter = baseDirection;
        }
        else
        {
            awayFromHitter.Normalize();
        }

        Vector3 blendedDirection = Vector3.Lerp(baseDirection, awayFromHitter, directionalInfluence);
        blendedDirection.y = 0f;
        blendedDirection.Normalize();

        Vector3 clampedDirection = Vector3.RotateTowards(baseDirection, blendedDirection, maxSidewaysAngle * Mathf.Deg2Rad, 0f);

        return clampedDirection.normalized;
    }

    private void ApplyBallForce(Vector3 forwardDirection, float forwardForce, float upForce)
    {
        Vector3 forceVector = (forwardDirection * forwardForce) + (Vector3.up * upForce);
        rb.AddForce(forceVector, ForceMode.Impulse);
    }
}