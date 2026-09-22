using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Animator scoreAnimator;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject loseText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Display initial score on game start
        UpdateScoreDisplay();

        // Ensure panels are hidden at match start
        if (scorePanel != null) scorePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the score text display.
    /// </summary>
    public void UpdateScoreDisplay()
    {
        if (scoreText != null && Player.instance != null && EnemyAI.instance != null)
        {
            scoreText.text = $"{Player.instance.Points} : {EnemyAI.instance.Points}";
        }
    }

    /// <summary>
    /// Triggers the scoring banner display and animation.
    /// </summary>
    public void PlayScoreAnimation()
    {
        StartCoroutine(ShowAndHideScoreRoutine());
    }

    private IEnumerator ShowAndHideScoreRoutine()
    {
        if (scorePanel != null) scorePanel.SetActive(true);

        if (scoreAnimator != null)
        {
            scoreAnimator.SetTrigger("ShowScore");
        }

        yield return new WaitForSeconds(2f);

        if (scorePanel != null) scorePanel.SetActive(false);
    }

    /// <summary>
    /// Displays the final win/lose animation banner.
    /// </summary>
    public void ShowGameOverScreen(string winnerName)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
            switch (winnerName)
            {
                case "Player":
                    winText.SetActive(true);
                    loseText.SetActive(false);
                    break;
                case "Enemy":
                    winText.SetActive(false);
                    loseText.SetActive(true);
                    break;
                default:
                    Debug.LogWarning("Unknown Winner string passed to ShowGameOverScreen");
                    break;
            }
        
    }

    /// <summary>
    /// Attach this method to your UI Restart Button's OnClick() event.
    /// </summary>
    public void BackToMainMenu()
    {
        Time.timeScale = 1f; // Always restore game speed when reloading
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with your actual main menu scene name
    }
}