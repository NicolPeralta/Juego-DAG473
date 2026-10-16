using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{

    public GameObject pauseCanvas;
    public GameObject optionsCanvas;
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

    public void OnButton_OpenOptions()
    {
        if (!isPaused)
        {
            Pause();
        }

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(true);
    }

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