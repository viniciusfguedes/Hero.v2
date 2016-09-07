//======================================
/*
@autor ktk.kumamoto
@date 2015.4.21 create
@note Button_ResetScene
*/
//======================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class Button_ResetScene : MonoBehaviour {

	private string SceneName;

	void OnGUI()
	{
		if(GUI.Button(new Rect(10, 10, 100, 30), "ResetScene")){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
	}
}