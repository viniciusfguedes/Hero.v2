using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class LevelMenuController : MonoBehaviour {

    public GameObject LoadingText;
    public GameObject LevelContainer;
    private PreferencesController preferences;

    void Start()
    {
        this.preferences = GameObject.Find("PreferencesController").GetComponent<PreferencesController>();
        this.SetStars();
    }

    private void SetStars()
    {
        Sprite starPrinted = Resources.Load<Sprite>("star-printed");

        for (int index = 1; index <= this.preferences.Level001Stars; index++)
            GameObject.Find("Level001_Star00" + index).GetComponent<Image>().sprite = starPrinted;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("000_StartMenu");
    }

    IEnumerator LoadLevel001Async()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("001_level");

        while (!async.isDone)
            yield return null;
    }

    private void ShowLoading()
    {
        this.LoadingText.SetActive(true);
        this.LevelContainer.SetActive(false);
    }

    public void LoadLevel001()
    {
        this.ShowLoading();
        StartCoroutine(LoadLevel001Async());
    }
}
