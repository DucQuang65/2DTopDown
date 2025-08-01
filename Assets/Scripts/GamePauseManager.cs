using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    public static void PauseGame()
    {
        IsPaused = true;
    }

    public static void ResumeGame()
    {
        IsPaused = false;
    }
}
