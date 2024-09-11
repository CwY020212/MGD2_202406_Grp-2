using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text leaderboardText; // For displaying the top 5 scores
    public GameObject leaderboardPanel; // The panel that shows the leaderboard

    private List<float> leaderboardScores = new List<float>();

    private void Start()
    {
        LoadLeaderboard();
        DisplayTopFiveScores();

        leaderboardPanel.SetActive(false);
    }

    /// <summary>
    /// Will load a new scene upon being called
    /// </summary>
    /// <param name="levelName">The name of the level we want
    /// to go to</param>
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

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
    private void DisplayTopFiveScores()
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
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
