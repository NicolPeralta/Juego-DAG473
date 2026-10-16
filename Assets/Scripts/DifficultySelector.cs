using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    [SerializeField] private GameObject difficultyCanvas; // asignar en Inspector

    void Awake()
    {
        if (difficultyCanvas != null) difficultyCanvas.SetActive(false);
    }

    // Mostrar / ocultar (para asignar desde Button -> OnClick() en el Inspector)
    public void Show() => difficultyCanvas?.SetActive(true);
    public void Hide() => difficultyCanvas?.SetActive(false);

    // Métodos públicos sin parámetros para asignar desde Button -> OnClick()
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
}