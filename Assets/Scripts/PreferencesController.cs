using System;
using UnityEngine;

public class PreferencesController : MonoBehaviour
{
    private bool? showTutorial;
    private float? musicVolume;
    private float? effectsVolume;
    private int? level001Stars;

    private bool? bestTime;
    private bool? diamondCollected;

    public bool ShowTutorial
    {
        get
        {
            if(!this.showTutorial.HasValue)
                this.showTutorial = Convert.ToBoolean(PlayerPrefs.GetInt("Tutorial", 1));

            return this.showTutorial.Value;
        }
        set
        {
            this.showTutorial = value;
            PlayerPrefs.SetInt("Tutorial", Convert.ToInt32(value));
        }
    }

    public float MusicVolume
    {
        get
        {
            if (!this.musicVolume.HasValue)
                this.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);

            return this.musicVolume.Value;
        }
        set
        {
            this.musicVolume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }

    public float EffectsVolume
    {
        get
        {
            if (!this.effectsVolume.HasValue)
                this.effectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 1);

            return this.effectsVolume.Value;
        }
        set
        {
            this.effectsVolume = value;
            PlayerPrefs.SetFloat("EffectsVolume", value);
        }
    }

    public int Level001Stars
    {
        get
        {
            if (!this.level001Stars.HasValue)
                this.level001Stars = PlayerPrefs.GetInt("level001Stars", 0);

            return this.level001Stars.Value;
        }
        set
        {
            this.level001Stars = value;
            PlayerPrefs.SetInt("level001Stars", value);
        }
    }

    public bool BestTime
    {
        get
        {
            if (!this.bestTime.HasValue)
                this.bestTime = Convert.ToBoolean(PlayerPrefs.GetInt("BestTime", 0));

            return this.bestTime.Value;
        }
        set
        {
            this.bestTime = value;
            PlayerPrefs.SetInt("BestTime", Convert.ToInt32(value));
        }
    }

    public bool DiamondCollected
    {
        get
        {
            if (!this.diamondCollected.HasValue)
                this.diamondCollected = Convert.ToBoolean(PlayerPrefs.GetInt("DiamondCollected", 0));

            return this.diamondCollected.Value;
        }
        set
        {
            this.diamondCollected = value;
            PlayerPrefs.SetInt("DiamondCollected", Convert.ToInt32(value));
        }
    }
}
