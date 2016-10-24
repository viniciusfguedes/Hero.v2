using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour {

    public GameObject PauseMenu;

    public Slider SliderMusicVolume;
    public Slider SliderEffectsVolume;

    private PreferencesController preferences;

    void Start()
    {
        this.preferences = GameObject.Find("PreferencesController").GetComponent<PreferencesController>();
        this.SliderMusicVolume.onValueChanged.AddListener(delegate { PauseMenuMusicVolumeChanged(); });
        this.SliderEffectsVolume.onValueChanged.AddListener(delegate { PauseMenuEffectsVolumeChanged(); });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            this.PauseMenuDoneClick();
    }

    /// <summary>
    /// Exibe o submenu de opções
    /// </summary>
    public void PauseMenuOpenClick()
    {
        this.SliderMusicVolume.value = this.preferences.MusicVolume;
        this.SliderEffectsVolume.value = this.preferences.EffectsVolume;

        this.PauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    /// <summary>
    /// Iniciar novamente a fase atual
    /// </summary>
    public void PauseMenuRestartClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    /// <summary>
    /// Controle do volume da música de fundo
    /// </summary>
    public void PauseMenuMusicVolumeChanged()
    {
        this.preferences.MusicVolume = this.SliderMusicVolume.value;
    }

    /// <summary>
    /// Controle do volume dos efeitos sonoros
    /// </summary>
    public void PauseMenuEffectsVolumeChanged()
    {
        this.preferences.EffectsVolume = this.SliderEffectsVolume.value;
    }

    /// <summary>
    /// Retorna para a seleção de fases
    /// </summary>
    public void PauseMenuQuitClick()
    {
        SceneManager.LoadScene("000_LevelMenu");
        Time.timeScale = 1;
    }

    /// <summary>
    /// Retorna para o jogo
    /// </summary>
    public void PauseMenuDoneClick()
    {
        this.PauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
}
