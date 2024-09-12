using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text leaderboardText; // For displaying the top 5 scores
    public GameObject leaderboardPanel; // The panel that shows the leaderboard
    public GameObject SoundPanel; // The panel that shows the leaderboard
    public Slider bgmSlider; // The slider for controlling the volume
    public Slider sfxSlider; // Slider for controlling SFX volume
    public GameObject MainPanel;

    [Header("Audio")]
    public AudioSource bgmAudioSource; // The AudioSource component for BGM
    public AudioSource sfxAudioSource; // The AudioSource component for SFX
    public AudioClip clickSound; // The sound to play on click

    private List<float> leaderboardScores = new List<float>();

    private void Start()
    {
        LoadLeaderboard();
        DisplayTopFiveScores();

        leaderboardPanel.SetActive(false);
        SoundPanel.SetActive(false);

        float savedBGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        SetBGMVolume(savedBGMVolume);
        SetSFXVolume(savedSFXVolume);

        // Set slider values and add listeners
        if (bgmSlider != null)
        {
            bgmSlider.value = savedBGMVolume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    /// <summary>
    /// Will load a new scene upon being called
    /// </summary>
    /// <param name="levelName">The name of the level we want
    /// to go to</param>
    public void LoadLevel(string levelName)
    {
        PlayClickSound();
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
        PlayClickSound();
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            MainPanel.SetActive(false);
        }
    }

    // Closes the leaderboard panel
    public void CloseLeaderboard()
    {
        PlayClickSound();
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
            MainPanel.SetActive(true);
        }
    }

    public void CloseGame()
    {
        PlayClickSound();
        Application.Quit();
    }

    private void PlayClickSound()
    {
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }
    }

    public void LoadSoundPanel()
    {
        PlayClickSound();
        SoundPanel.SetActive(true);
        MainPanel.SetActive(false);
    }

    public void CloseSoundPanel()
    {
        PlayClickSound();
        if (SoundPanel != null)
        {
            SoundPanel.SetActive(false);
            MainPanel.SetActive(true);
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = volume;
            PlayerPrefs.SetFloat("BGMVolume", volume);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = volume;
            PlayerPrefs.SetFloat("SFXVolume", volume);
            PlayerPrefs.Save();
        }
    }
}

