using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Asigna aquí el Canvas (GameObject) del menú de pausa. Debe estar DESACTIVADO en la escena.")]
    public GameObject pauseCanvas;

    [Tooltip("Panel de opciones (opcional). Asignar en inspector si existe.")]
    public GameObject optionsCanvas;

    [Header("Opciones")]
    public bool pauseAudio = false;
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Awake()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    void Update()
    {
        bool toggle = false;

        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.escapeKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                toggle = true;
        }
        else
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) || UnityEngine.Input.GetKeyDown(KeyCode.Space))
                toggle = true;
        }

        if (toggle) TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);

        Time.timeScale = 0f;

        if (pauseAudio) AudioListener.pause = true;
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);

        Time.timeScale = 1f;

        if (pauseAudio) AudioListener.pause = false;
    }

    // Abrir panel de opciones desde botón del canvas de pausa
    public void OnButton_OpenOptions()
    {
        if (!isPaused)
        {
            Pause(); // asegurar que estamos en pausa
        }

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(true);
    }

    // Volver desde opciones al menu de pausa
    public void OnButton_CloseOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
    }

    public void OnButton_Resume() => Resume();

    public void OnButton_Quit()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}   