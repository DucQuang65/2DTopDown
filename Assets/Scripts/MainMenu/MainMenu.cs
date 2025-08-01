
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void QuitGame()
    {
        Debug.LogFormat("Quited");
        Application.Quit();
    }
    public void BackGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
