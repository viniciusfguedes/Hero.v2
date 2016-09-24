using System;
using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public bool PlayerWon;
    public float CountdownTimer;
    public Text CurrentCountdownText;

    void Update()
    {
        //this.CountdownTimer -= Time.deltaTime;

        if (this.CountdownTimer > 0)
        {
            this.CurrentCountdownText.text = string.Format("{0}:{1}",
                                                           Mathf.Floor(this.CountdownTimer / 60).ToString("00"),
                                                           (this.CountdownTimer % 60).ToString("00"));
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if(this.PlayerWon)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
