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
    [SerializeField] private int maxScore = 15;


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

        int pPoints = Player.instance.Points;
        int ePoints = EnemyAI.instance.Points;

        // 2. Check for Winner BEFORE displaying score animation/banner
        if (pPoints >= maxScore || ePoints >= maxScore)
        {
            string winnerName = pPoints >= maxScore ? "Player" : "Enemy";

            // Update score text one last time so the final point shows on screen
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScoreDisplay();
                UIManager.Instance.ShowGameOverScreen(winnerName);
            }

            // Freeze physics and stop round loop
            Time.timeScale = 0f;
        }
        else
        {
            // 3. Match continues: update score UI & trigger point animation
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScoreDisplay();
                UIManager.Instance.PlayScoreAnimation();
            }

            // 4. Pause during banner display
            yield return new WaitForSeconds(scorePauseDuration);

            // 5. Reset Positions for next serve
            ResetRoundPositions(scorer);
            isResettingRound = false;
        }
    }

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