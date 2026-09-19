using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [Header("Blob Shadow Settings")]
    public GameObject shadow;
    public float offset = 0.5f;
    public float maxShadowDistance = 25f;

    [Header("Bump Hit Settings (Grounded Player)")]
    [SerializeField] private float bumpUpForce = 8f;
    [SerializeField] private float bumpForwardForce = 4f;

    [Header("Spike Hit Settings (Jumping Player)")]
    [SerializeField] private float spikeUpForce = 2f;
    [SerializeField] private float spikeForwardForce = 15f;

    [SerializeField] private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Ensure continuous collision detection to prevent ball passing through net/floor
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        UpdateBlobShadow();
    }

    private void UpdateBlobShadow()
    {
        if (shadow == null) return;

        // Cast a ray down from just below the ball
        Ray downRay = new Ray(transform.position - (Vector3.up * offset), Vector3.down);

        if (Physics.Raycast(downRay, out RaycastHit hit, maxShadowDistance))
        {
            if (!shadow.activeSelf) shadow.SetActive(true);

            // Align shadow position with the ground point, adding a slight offset to prevent Z-fighting
            shadow.transform.position = hit.point + (Vector3.up * 0.01f);

            // Optional: Align shadow rotation to match ground slope
            shadow.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
        else
        {
            // Disable shadow if ball is out of reach/too high
            if (shadow.activeSelf) shadow.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if hit object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (player != null)
            {
                rb.useGravity = true; // Ensure gravity is enabled for the ball
                // Direction player is facing
                Vector3 playerForward = collision.transform.forward;

                // Reset existing velocity so the hit feels consistent
                rb.linearVelocity = Vector3.zero;

                if (player.currentState == Player.PlayerState.Jumping)
                {
                    // Spike force (fast & forward)
                    ApplyBallForce(playerForward, spikeForwardForce, spikeUpForce);
                }
                else
                {
                    // Bump force (high & arc)
                    ApplyBallForce(playerForward, bumpForwardForce, bumpUpForce);
                }
            }
        }
        if (collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Wall"))
        {
            // Reset ball's velocity when it hits the floor
            Referee.instance.PositionBall("Player");
            Referee.instance.PositionPlayer();
        }
    }

    private void ApplyBallForce(Vector3 forwardDirection, float forwardForce, float upForce)
    {
        Vector3 forceVector = (forwardDirection.normalized * forwardForce) + (Vector3.up * upForce);
        rb.AddForce(forceVector, ForceMode.Impulse);
    }
}