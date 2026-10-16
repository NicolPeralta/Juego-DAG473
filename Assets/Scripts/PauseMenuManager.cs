using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;   // asignar en Inspector
    [SerializeField] private GameObject optionsCanvas; // opciones accesibles desde pausa (opcional)

    private bool isPaused = false;

    void Awake()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // Métodos públicos para Button -> OnClick()
    public void TogglePause()
    {
        if (!isPaused) Pause();
        else Resume();
    }

    public void Pause()
    {
        isPaused = true;
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}       