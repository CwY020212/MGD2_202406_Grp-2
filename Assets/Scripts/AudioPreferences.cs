using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioPreferences
{
    public static float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat("BGMVolume", 1f);
    }

    public static float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public static void SetBGMVolume(float volume)
    {
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public static void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
}