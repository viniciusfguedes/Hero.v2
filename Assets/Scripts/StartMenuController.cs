using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public GameObject OptionsMenu;
    public Toggle ToggleTutorial;
    public Slider SliderMusicVolume;
    public Slider SliderEffectsVolume;

    private PreferencesController preferences;

    void Start()
    {
        this.preferences = GameObject.Find("PreferencesController").GetComponent<PreferencesController>();

        this.GetComponent<AudioSource>().volume = this.preferences.EffectsVolume;
        Camera.main.GetComponent<AudioSource>().volume = this.preferences.MusicVolume;

        this.SliderMusicVolume.onValueChanged.AddListener(delegate { OptionsMusicVolumeChanged(); });
        this.SliderEffectsVolume.onValueChanged.AddListener(delegate { OptionsEffectsVolumeChanged(); });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    /// <summary>
    /// Carrega o menu de fases
    /// </summary>
    public void StartButtonClick()
    {
        SceneManager.LoadScene("000_LevelMenu");
    }

    /// <summary>
    /// Exibe o submenu de opções
    /// </summary>
    public void OptionsButtonClick()
    {
        this.ToggleTutorial.isOn = this.preferences.ShowTutorial;
        this.SliderMusicVolume.value = this.preferences.MusicVolume;
        this.SliderEffectsVolume.value = this.preferences.EffectsVolume;

        this.OptionsMenu.SetActive(true);
    }

    /// <summary>
    /// Reseta as configurações e o progresso do jogo
    /// </summary>
    public void OptionsResetClick()
    {
        PlayerPrefs.DeleteAll();
        
        //Tutorial
        this.ToggleTutorial.isOn = true;
        this.preferences.ShowTutorial = true;

        //Music Volume
        this.SliderMusicVolume.value = 1;
        this.preferences.MusicVolume = 1;

        //Effects Volume
        this.SliderEffectsVolume.value = 1;
        this.preferences.EffectsVolume = 1;

        //Level goals
    }

    /// <summary>
    /// Exibição do tutorial
    /// </summary>
    public void OptionsTutorialClick()
    {
        this.preferences.ShowTutorial = this.ToggleTutorial.isOn;
    }

    /// <summary>
    /// Controle do volume da música de fundo
    /// </summary>
    public void OptionsMusicVolumeChanged()
    {
        this.preferences.MusicVolume = this.SliderMusicVolume.value;
        Camera.main.GetComponent<AudioSource>().volume = this.SliderMusicVolume.value;
    }

    /// <summary>
    /// Controle do volume dos efeitos sonoros
    /// </summary>
    public void OptionsEffectsVolumeChanged()
    {
        this.preferences.EffectsVolume = this.SliderEffectsVolume.value;

        AudioSource[] audioSources = GameObject.Find("Root").GetComponentsInChildren<AudioSource>();

        foreach (AudioSource audioSource in audioSources)
            if(audioSource.gameObject.tag != "MainCamera")
                audioSource.volume = this.SliderEffectsVolume.value;
    }

    /// <summary>
    /// Fecha o submenu de opções
    /// </summary>
    public void OptionsDoneClick()
    {
        this.OptionsMenu.SetActive(false);
    }
}
