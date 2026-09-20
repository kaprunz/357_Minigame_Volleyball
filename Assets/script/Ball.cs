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

    [Header("Direction Settings")]
    [SerializeField, Range(0f, 1f)]
    private float directionalInfluence = 0.4f;

    [SerializeField, Range(0f, 60f)]
    private float maxSidewaysAngle = 45f;

    [SerializeField] private Rigidbody rb;

    public GameObject lastTouchedPlayer { get; private set; }

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
        // 1. Human Player (at -Z court, hits towards +Z / Enemy court)
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                lastTouchedPlayer = collision.gameObject;
                HandleHit(collision.transform, Vector3.forward, player.currentState == Player.PlayerState.Jumping);
            }
        }
        // 2. Enemy AI (at +Z court, hits towards -Z / Player court)
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                lastTouchedPlayer = collision.gameObject;
                // Changed to Vector3.back (-Z) so the ball travels across the net toward the Player
                HandleHit(collision.transform, Vector3.back, enemy.currentPhysicalState == Player.PlayerState.Jumping);
            }
        }
        // 3. Out of Bounds / Floor Collision
        else if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Wall"))
        {
            if (Referee.instance != null)
            {
                Referee.instance.PositionBall("Player");
                Referee.instance.PositionPlayer();
            }
        }
    }

    private void HandleHit(Transform hitterTransform, Vector3 targetBaseDirection, bool isJumping)
    {
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;

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