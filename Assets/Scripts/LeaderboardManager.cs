using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance; // Singleton instance

    [Header("UI References")]
    public TMP_Text highScoreText; // For displaying the high score
    //public TMP_Text leaderboardText; // For displaying the top 5 scores
    //public GameObject leaderboardPanel; // The panel that shows the leaderboard

    UIHandler highScore;

    private List<float> leaderboardScores = new List<float>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the leaderboard manager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        highScore = FindObjectOfType<UIHandler>();
        LoadLeaderboard();
        //DisplayHighScore();
        //DisplayTopFiveScores();
    }

    // Finds the necessary UI elements at runtime
    /*private void FindUIElements()
    {
        if (highScoreText == null)
        {
            highScoreText = GameObject.FindWithTag("HighScoreText").GetComponent<TMP_Text>();
        }

        if (leaderboardText == null)
        {
            leaderboardText = GameObject.Find("LeaderboardText").GetComponent<TMP_Text>();
        }

        if (leaderboardPanel == null)
        {
            leaderboardPanel = GameObject.Find("LeaderboardPanel");
        }

        // Hide the leaderboard panel initially
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }*/

    // Saves the current score to the leaderboard
    public void SaveScore(float score)
    {
        leaderboardScores.Add(score);
        leaderboardScores.Sort((a, b) => b.CompareTo(a)); // Sort descending

        if (leaderboardScores.Count > 5) // Keep only top 5 scores
        {
            leaderboardScores.RemoveAt(leaderboardScores.Count - 1);
        }

        SaveLeaderboard();
        highScore.DisplayHighScore();
        //DisplayTopFiveScores();
    }

    // Saves the leaderboard to PlayerPrefs
    private void SaveLeaderboard()
    {
        for (int i = 0; i < leaderboardScores.Count; i++)
        {
            PlayerPrefs.SetFloat($"LeaderboardScore_{i}", leaderboardScores[i]);
        }
        PlayerPrefs.SetInt("LeaderboardCount", leaderboardScores.Count);
        PlayerPrefs.Save();
    }

    // Loads the leaderboard from PlayerPrefs
    private void LoadLeaderboard()
    {
        leaderboardScores.Clear();
        int count = PlayerPrefs.GetInt("LeaderboardCount", 0);
        for (int i = 0; i < count; i++)
        {
            leaderboardScores.Add(PlayerPrefs.GetFloat($"LeaderboardScore_{i}", 0f));
        }
    }


    // Displays the top five scores on the leaderboardText UI element
    /*private void DisplayTopFiveScores()
    {
        if (leaderboardText == null) return;

        leaderboardText.text = "Top 5 Scores:\n";
        for (int i = 0; i < leaderboardScores.Count; i++)
        {
            leaderboardText.text += $"{i + 1}. {leaderboardScores[i]:0}\n";
        }
    }

    // Opens the leaderboard panel
    public void OpenLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
        }
    }

    // Closes the leaderboard panel
    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }*/
}
