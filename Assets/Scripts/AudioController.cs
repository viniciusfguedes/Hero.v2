using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

    void Awake()
    {
        this.GetComponent<AudioSource>().volume = GameObject.Find("PreferencesController").GetComponent<PreferencesController>().EffectsVolume;
    }
}
