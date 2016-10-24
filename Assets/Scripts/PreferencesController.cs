using System;
using UnityEngine;

public class PreferencesController : MonoBehaviour
{
    private bool? showTutorial;
    private float? musicVolume;
    private float? effectsVolume;

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
}
