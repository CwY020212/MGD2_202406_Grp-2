using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class ObstacleBehaviour : MonoBehaviour
{
    [Tooltip("How long to wait before restarting the game")]
    public float waitTime = 2.0f;

    private PlayerBehavior plyrScore;
    UIHandler highScore;
  
    private void Start()
    {
        plyrScore = FindAnyObjectByType<PlayerBehavior>();
        highScore = FindObjectOfType<UIHandler>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerBehavior player = collision.gameObject.GetComponent<PlayerBehavior>();
        if (player != null)
        {
            player.ShieldPower();

            // If the player is not destroyed, we don't need to restart the game
            if (!player.hasShield)
            {
                Invoke("ResetGame", waitTime);
            }
        }
    }

    /// Will restart the currently loaded level
    private void ResetGame()
    {
        // Restarts the current level
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //Bring up restart menu
        var go = GetGameOverMenu();
        go.SetActive(true);
        
        plyrScore.scoreText.gameObject.SetActive(false);

        SavePlayerScore();
        highScore.DisplayHighScore();
    }

     public GameObject GetGameOverMenu()
    {
        var canvas = GameObject.Find("Canvas").transform;
        return canvas.Find("Game Over").gameObject;

    }

    public void SavePlayerScore()
    {
        LeaderboardManager.Instance.SaveScore(plyrScore.Score);
    }
}
