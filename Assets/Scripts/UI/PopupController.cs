using UnityEngine;
using UnityEngine.SceneManagement;

public class PopupController :MonoBehaviour{
    public void LoadMainScene()
    {

         SceneManager.LoadScene(0);
    }
    public void LoadGameScene()
    {

         SceneManager.LoadScene(1);
    }
}