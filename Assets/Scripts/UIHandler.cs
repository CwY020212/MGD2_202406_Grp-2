using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    private PlayerBehavior plyrScore;

    [Header("UI References")]
    public TMP_Text highScoreText; // For displaying the high score

    private List<float> leaderboardScores = new List<float>();

    void Start()
    {
        plyrScore = FindAnyObjectByType<PlayerBehavior>();

        LoadLeaderboard();
        DisplayHighScore();

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

    public void DisplayHighScore()
    {
        LoadLeaderboard();

        if (leaderboardScores[0] <= plyrScore.Score)
        {
            highScoreText.text = $"High Score:\n {plyrScore.Score:0}";
        }
        else if (leaderboardScores.Count > 0 && highScoreText != null)
        {
            highScoreText.text = $"High Score:\n {leaderboardScores[0]:0}";
        }
        else if (highScoreText != null)
        {
            highScoreText.text = "High Score:\n 0";
        }
    }
}
