using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<AudioSource>().volume = GameObject.Find("PreferencesController").GetComponent<PreferencesController>().EffectsVolume;
    }

    public void PlaySound()
    {
        this.GetComponent<AudioSource>().Play();
    }
}