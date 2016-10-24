using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenuController : MonoBehaviour {

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("000_StartMenu");
    }

    public void LoadLevel001()
    {
        SceneManager.LoadScene("001_level");
    }
}
