using System.Collections;
using UnityEngine;
using TMPro; // Standard Unity TextMeshPro

public class Referee : MonoBehaviour
{
    public static Referee instance;

    [Header("Transform References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform enemyTransform;
    [SerializeField] private Transform ballTransform;

    [Header("Serve Position Offsets")]
    [SerializeField] private Vector3 playerSpawnPos = new Vector3(0f, 1f, -5f);
    [SerializeField] private Vector3 enemySpawnPos = new Vector3(0f, 1f, 5f);
    [SerializeField] private Vector3 playerServeBallOffset = new Vector3(0f, 3f, -4f);
    [SerializeField] private Vector3 enemyServeBallOffset = new Vector3(0f, 3f, 4f);

    [Header("Timing Settings")]
    [SerializeField] private float scorePauseDuration = 1.2f;


    private bool isResettingRound = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Initial serve setup on game start (Player serves first)
        ResetRoundPositions("Player");
    }

    /// <summary>
    /// Unified point handler called by Ball.cs
    /// </summary>
    public void ScorePoint(string scorer)
    {
        if (isResettingRound) return;
        StartCoroutine(PointScoredRoutine(scorer));
    }

    private IEnumerator PointScoredRoutine(string scorer)
    {
        isResettingRound = true;

        // 1. Update Scores
        if (scorer == "Player") Player.instance.Points++;
        else if (scorer == "Enemy") EnemyAI.instance.Points++;

        // 2. Show "POINT!" banner/UI
        // if (pointScoredUI != null) pointScoredUI.SetActive(true);

        // 3. Pause for brief second (Pikachu Volleyball style)
        yield return new WaitForSeconds(scorePauseDuration);

        // 4. Hide UI banner
        // if (pointScoredUI != null) pointScoredUI.SetActive(false);

        // 5. Reset Positions (Unified method call)
        ResetRoundPositions(scorer);

        isResettingRound = false;
    }

    /// <summary>
    /// Unified method to reposition Player, Enemy, and Ball based on who serves.
    /// </summary>
    /// <param name="servingTeam">"Player" or "Enemy"</param>
    public void ResetRoundPositions(string servingTeam)
    {
        // Freeze ball physics and disable gravity until it gets hit
        Rigidbody ballRb = ballTransform.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.isKinematic = true;
            ballRb.useGravity = false;
        }

        // Reset Player and Enemy positions
        playerTransform.position = playerSpawnPos;
        enemyTransform.position = enemySpawnPos;

        // Position ball according to who serves
        if (servingTeam == "Player")
        {
            ballTransform.position = playerServeBallOffset;
        }
        else
        {
            ballTransform.position = enemyServeBallOffset;
        }
    }
}