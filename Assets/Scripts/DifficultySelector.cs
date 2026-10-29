using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private GameObject difficultyCanvas;
    [SerializeField] private MainMenuManager mainMenuManager;

    void Awake()
    {
        if (difficultyCanvas != null) difficultyCanvas.SetActive(false);
    }

    public void Show() => difficultyCanvas?.SetActive(true);
    public void Hide() => difficultyCanvas?.SetActive(false);
    public void SetEasy()   => SetDifficulty(GameSettings.Difficulty.Facil);
    public void SetMedium() => SetDifficulty(GameSettings.Difficulty.Medio);
    public void SetHard()   => SetDifficulty(GameSettings.Difficulty.Dificil);

    private void SetDifficulty(GameSettings.Difficulty d)
    {
        GameSettings.ApplyDifficulty(d);
        StartGame();
    }

    private void StartGame()
    {
        string scene = string.IsNullOrEmpty(GameSettings.GameSceneName) ? "Game" : GameSettings.GameSceneName;
        SceneManager.LoadScene(scene);
    }

    // Método público para enlazar desde un botón UI: vuelve al menú principal
    public void BackToMainMenu()
    {
        Hide();
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
        else
        {
            Debug.LogWarning("DifficultySelector: `mainMenuManager` no asignado. Asigna la referencia en el Inspector.");
        }
    }
}