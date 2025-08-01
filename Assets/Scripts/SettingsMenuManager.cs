using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{

    public void OpenSettings()
    {
        GamePauseManager.PauseGame();
    }

    public void CloseSettings()
    {
        GamePauseManager.ResumeGame();
    }
}
