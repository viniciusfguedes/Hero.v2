﻿using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public bool GameOver;
    public bool PlayerWon;
    public bool ShowTutorial;
    public bool ShowWeaponTutorial;
    public bool ShowLavaWallTutorial;

    public float CountdownTimer;
    public Text CurrentCountdownText;

    public GameObject Star001;
    public GameObject Star002;
    public GameObject Star003;

    public GameObject WonPanelGameObject;
    public GameObject OverPanelGameObject;

    public GameObject TutorialGameObject;
    public GameObject FlyingTutorialGameObject;
    public GameObject WeaponTutorialGameObject;
    public GameObject LavaWallTutorialGameObject;
    public GameObject HorizontalMovementTutorialGameObject;

    private bool isShowingTutorial;
    private bool flyingTutorialDone;
    private bool weaponTutorialDone;
    private bool lavaWallTutorialDone;
    private bool horizontalMovementTutorialDone;
    private PreferencesController preferences;

    void Start()
    {
        this.preferences = GameObject.Find("PreferencesController").GetComponent<PreferencesController>();
        Camera.main.GetComponent<AudioSource>().volume = this.preferences.MusicVolume;

        this.ShowTutorial = this.preferences.ShowTutorial;
        this.TutorialGameObject.SetActive(false);
        float minute = Mathf.Floor(this.CountdownTimer / 60);
        float seconds = this.CountdownTimer % 60;

        this.CurrentCountdownText.text = string.Format("{0}:{1}",
                                                       minute.ToString("00"),
                                                       (seconds == 60f ? 0 : seconds).ToString("00"));
    }

    void Update()
    {
        if(this.ShowTutorial)
        {
            if(!this.horizontalMovementTutorialDone)
            {
                Time.timeScale = 0;
                this.isShowingTutorial = true;
                this.horizontalMovementTutorialDone = true;

                this.TutorialGameObject.SetActive(true);
                this.HorizontalMovementTutorialGameObject.SetActive(true);
            }
            else if(!this.flyingTutorialDone && !this.isShowingTutorial)
            {
                Time.timeScale = 0;
                this.isShowingTutorial = true;
                this.flyingTutorialDone = true;

                this.TutorialGameObject.SetActive(true);
                this.FlyingTutorialGameObject.SetActive(true);
            }
            else if(this.ShowWeaponTutorial && !this.weaponTutorialDone && !this.isShowingTutorial)
            {
                Time.timeScale = 0;
                this.isShowingTutorial = true;
                this.weaponTutorialDone = true;

                this.TutorialGameObject.SetActive(true);
                this.WeaponTutorialGameObject.SetActive(true);
            }
            else if (this.ShowLavaWallTutorial && !this.isShowingTutorial)
            {
                Time.timeScale = 0;
                this.isShowingTutorial = true;
                this.ShowLavaWallTutorial = false;

                this.TutorialGameObject.SetActive(true);
                this.LavaWallTutorialGameObject.SetActive(true);
            }
        }

        if (!this.isShowingTutorial)
        {
            this.CountdownTimer -= Time.deltaTime;

            if (this.CountdownTimer > 0)
            {
                float minute = Mathf.Floor(this.CountdownTimer / 60);
                float seconds = this.CountdownTimer % 60;

                this.CurrentCountdownText.text = string.Format("{0}:{1}",
                                                               minute.ToString("00"),
                                                               (seconds == 60f ? 0 : seconds).ToString("00"));
            }
            else
                this.GameOver = true;

            if (this.PlayerWon)
            {
                Sprite starPrinted = Resources.Load<Sprite>("star-printed");

                //Fase concluída
                this.Star001.GetComponent<Image>().sprite = starPrinted;

                //Diamantes coletados
                if (this.preferences.DiamondCollected)
                    this.Star002.GetComponent<Image>().sprite = starPrinted;

                //Dentro de 2 minutos e meio
                if (this.preferences.BestTime)
                    this.Star003.GetComponent<Image>().sprite = starPrinted;

                this.WonPanelGameObject.SetActive(true);
            }

            if (this.GameOver)
            {
                this.OverPanelGameObject.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    public void CloseHorizontalMovementTutorial()
    {
        this.HorizontalMovementTutorialGameObject.SetActive(false);
        this.TutorialGameObject.SetActive(false);

        this.isShowingTutorial = false;
        Time.timeScale = 1f;
    }

    public void CloseFlyingTutorial()
    {
        this.FlyingTutorialGameObject.SetActive(false);
        this.TutorialGameObject.SetActive(false);

        this.isShowingTutorial = false;
        Time.timeScale = 1f;
    }

    public void CloseWeaponTutorial()
    {
        this.WeaponTutorialGameObject.SetActive(false);
        this.TutorialGameObject.SetActive(false);

        this.isShowingTutorial = false;
        Time.timeScale = 1f;
    }

    public void CloseLavaWallTutorial()
    {
        this.LavaWallTutorialGameObject.SetActive(false);
        this.TutorialGameObject.SetActive(false);

        this.isShowingTutorial = false;
        Time.timeScale = 1f;
    }
}
