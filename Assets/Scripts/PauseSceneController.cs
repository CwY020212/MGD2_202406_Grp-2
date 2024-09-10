using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSceneController : MainMenuController
{
    public static bool paused;
    [Tooltip("Reference to the pause menu object to turn on/off")]
    public GameObject pauseMenu;

    /// <summary>
    /// Reloads our current level, effectively "restarting" the
    /// game
    /// </summary>
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /// <summary>
    /// Will turn our pause menu on or off
    /// </summary>
    /// <param name="isPaused"></param>
    public void SetPauseMenu(bool isPaused)
    {
        paused = isPaused;
        // If the game is paused, timeScale is 0, otherwise 1
        Time.timeScale = (paused) ? 0 : 1;
        pauseMenu.SetActive(paused);
    }
    void Start()
    {
        // Must be reset in Start or else game will be paused upon
        // restart
        SetPauseMenu(false);
    }

    #region Share Score via Twitter
    /// <summary>
    /// Web address in order to create a tweet
    /// </summary>
    private const string tweetTextAddress = "http://twitter.com/intent/tweet?text=";

    /// <summary>
    /// Where we want players to visit
    /// </summary>
    private string appStoreLink = "http://johnpdoran.com/";


    [Tooltip("Reference to the player for the score")]
    public PlayerBehavior player;
    /// <summary>
    /// Will open Twitter with a prebuilt tweet. When called on iOS or
    /// Android will open up Twitter app if installed
    /// </summary>
    public void TweetScore()
    {
        // Create contents of the tweet
        string tweet = "I got " + string.Format("{0:0}", player.Score)
        + " points in Seasons Of Wonder! Can you do better?";
        // Create the entire message
        string message = tweet + "\n" + appStoreLink;
        //Ensures string is URL friendly
        string url =
        UnityEngine.Networking.UnityWebRequest.EscapeURL(message);
        // Open the URL to create the tweet
        Application.OpenURL(tweetTextAddress + url);
    }
    #endregion
}
